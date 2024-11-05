using System.Text;
using ModDK.Extensions;

namespace ModDK {
    namespace FileFormats {
        struct LgpConstants {
            public const int HeaderCreatorSize = 12;
            public const string HeaderExpectedCreator = "\0\0SQUARESOFT";
            public const int TableOfContentsFilenameSize = 20;
            public const int TableOfContentsConflictSize = 2;
            public const int CRCSize = 3600;
            public const int LookupTableIndexSize = 2;
            public const int LoopupTableCountSize = 2;
            public const int LookupTableMaxIndex = 30;
            public const int ConflictTableNumberOfConflictsSize = 2;
            public const int ConflictTableEntryNumberOfLocationsSize = 2;
            public const int ConflictTableEntryFolderNameSize = 128;
            public const int ConflictTableEntryTableOfContentsIndexSize = 2;
            public const int FileEntryFilenameSize = 20;
            public const int TerminatorSize = 14;
            public const string TerminatorExpectedDescriptor = "FINAL FANTASY7";

            public static uint HeaderSize {
                get {
                    return HeaderCreatorSize;
                }
            }

            public static uint TableOfContentEntrySize {
                get {
                    return TableOfContentsFilenameSize + sizeof(uint) + sizeof(byte) + sizeof(ushort);
                }
            }
        }

        struct LgpTableOfContentsEntry {
            public byte[] Filename = new byte[LgpConstants.TableOfContentsFilenameSize];
            public uint FileOffset;
            public byte Check;
            public ushort ConflictIndex;
            public string? Path;

            public bool HasExtendedFilePath {
                get { 
                    return ConflictIndex != 0; 
                }
            }

            public readonly string FullFilePath {
                get {
                    string encodedFilename = Encoding.ASCII.GetString(Filename.Where(b => b != 0x00).ToArray());
                    if (Path == null) {
                        return encodedFilename;
                    }

                    return $"{Path}/{encodedFilename}";
                }
            }

            public LgpTableOfContentsEntry() {}
        }

        struct LgpLookupTableEntry {
            public ushort NumberOfConflicts = 0;
            public ushort NumberOfLocations = 0;
            public byte[] FolderName = new byte[LgpConstants.ConflictTableEntryFolderNameSize];
            public ushort TableOfContentsIndex = 0;

            public LgpLookupTableEntry() {}
        }

        struct LgpFileEntry {
            public byte[] Filename = new byte[LgpConstants.FileEntryFilenameSize];
            public uint FileLength = 0;
            public List<byte> Data = new List<byte>();

            public LgpFileEntry() {}
        }

        struct LgpFile : IDisposable {
            public byte[] Creator = new byte[LgpConstants.HeaderCreatorSize];
            public uint FileCount;
            public List<LgpTableOfContentsEntry> TableOfContents = new List<LgpTableOfContentsEntry>();
            public byte[] CRC = new byte[LgpConstants.CRCSize];
            public List<byte> Terminator = new List<byte>();

            private Stream? _stream;
            private Stream Stream {
                readonly get => _stream ?? throw new Exception("Trying to read from Stream when it is null");
                set => _stream = value;
            }

            public readonly bool IsValid {
                get {
                    bool headerValid = Encoding.UTF8.GetString(Creator) == LgpConstants.HeaderExpectedCreator;
                    bool terminatorValid = Encoding.UTF8.GetString(Terminator.ToArray()) == LgpConstants.TerminatorExpectedDescriptor;

                    return headerValid && terminatorValid;
                }
            }

            private readonly bool ShouldReadLookupTable {
                get {
                    return TableOfContents.Any(entry => entry.HasExtendedFilePath);
                }
            }

            private readonly long CalculatedTableOfContentsStartPosition {
                get {
                    return LgpConstants.HeaderCreatorSize + sizeof(uint);
                }
            }

            private readonly long CalculatedCRCStartPosition {
                get {
                    return CalculatedTableOfContentsStartPosition + (FileCount * LgpConstants.TableOfContentEntrySize);
                }
            }

            public LgpFile() {}

            public readonly void Dispose() {
                Stream?.Dispose();
            }

            public static LgpFile FromStream(Stream s) {
                LgpFile file = new LgpFile {
                    Stream = s
                };

                file.ReadHeaderMeta();
                file.ReadTableOfContents();
                file.ReadCRC();

                if (file.ShouldReadLookupTable) {
                    file.ReadLookupTable();
                }
                
                file.ReadTerminator();
                return file;
            }

            public IEnumerable<(LgpTableOfContentsEntry, LgpFileEntry)> ReadFiles() {
                foreach (LgpTableOfContentsEntry entry in TableOfContents) {
                    yield return (entry, ReadFileFromTableOfContentsEntry(entry));
                }
            }

            private void ReadHeaderMeta() {
                Creator = Stream.ReadBytes(LgpConstants.HeaderCreatorSize);
                FileCount = Stream.ReadUInt32();
            }

            private void ReadTableOfContents() {
                if (Stream.Position != CalculatedTableOfContentsStartPosition) {
                    throw new Exception("Cannot read table of contents as reader is not at correct position");
                }

                foreach (int _ in Enumerable.Range(0, (int)FileCount)) {
                    TableOfContents.Add(ReadNextTableOfContentsEntry());
                }
            }

            private LgpTableOfContentsEntry ReadNextTableOfContentsEntry() {
                return new LgpTableOfContentsEntry {
                    Filename = Stream.ReadBytes(LgpConstants.TableOfContentsFilenameSize),
                    FileOffset = Stream.ReadUInt32(),
                    Check = (byte)Stream.ReadByte(),
                    ConflictIndex = Stream.ReadUInt16()
                };
            }

            private readonly void ReadLookupTable() {
                Stream.Seek(CalculatedCRCStartPosition + LgpConstants.CRCSize, SeekOrigin.Begin);
                long firstFileOffset = TableOfContents.OrderBy(entry => entry.FileOffset).First().FileOffset;
                
                ushort numberOfConflictingFileNames = Stream.ReadUInt16();
                foreach (int _a in Enumerable.Range(0, numberOfConflictingFileNames))
                {
                    ushort currentNumberOfConflicts = Stream.ReadUInt16();
                    foreach (int _b in Enumerable.Range(0, currentNumberOfConflicts))
                    {
                        byte[] folderName = Stream.ReadBytes(128);
                        ushort tableOfContentsIndex = Stream.ReadUInt16();

                        string folder = Encoding.ASCII.GetString(folderName.Where(b => b != '\0').ToArray());
                        
                        LgpTableOfContentsEntry entry = TableOfContents[tableOfContentsIndex];
                        entry.Path = folder;
                        TableOfContents[tableOfContentsIndex] = entry;
                    }
                }
            }

            private void ReadCRC() {
                if (Stream.Position != CalculatedCRCStartPosition) {
                    throw new Exception("Cannot read CRC as reader is not at correct position");
                }

                CRC = Stream.ReadBytes(LgpConstants.CRCSize);
            }

            private LgpFileEntry ReadFileFromTableOfContentsEntry(LgpTableOfContentsEntry entry) {
                Stream.Seek(entry.FileOffset, SeekOrigin.Begin);

                byte[] fileName = Stream.ReadBytes(LgpConstants.FileEntryFilenameSize);
                uint fileLength = Stream.ReadUInt32();
                byte[] fileData = Stream.ReadBytes((int)fileLength);

                return new LgpFileEntry {
                    Filename = fileName,
                    FileLength = fileLength,
                    Data = fileData.ToList()
                };
            }

            private void ReadTerminator() {
                LgpTableOfContentsEntry lastEntry = TableOfContents.OrderByDescending(entry => entry.FileOffset).First();
                Stream.Seek(lastEntry.FileOffset, SeekOrigin.Begin);
                Stream.ReadBytes(LgpConstants.FileEntryFilenameSize);
                
                uint fileLength = Stream.ReadUInt32();
                Stream.Seek(fileLength, SeekOrigin.Current);

                Terminator = Stream.ReadBytes((int)(Stream.Length - Stream.Position)).ToList();
            }
        }
    }
}