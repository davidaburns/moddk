using ModDK.Extensions;

namespace ModDK {
    namespace FileFormats {
        struct HrcConstants {
            public const int HeaderSectionSize = 3;
            public const int BoneEntrySectionSize = 4;
        }

        struct HrcBoneEntry {
            public string Name = "";
            public string Parent = "";
            public float Length = 0.0f;
            public int RsdFileCount = 0;
            public List<string> RsdFileNames = new List<string>();

            public HrcBoneEntry() {}

            public static HrcBoneEntry FromHRCFileSection(List<string> section) {
                if (section.Count != HrcConstants.BoneEntrySectionSize) {
                    throw new Exception("HRC Bone Entry Section is not a valid size");
                }

                HrcBoneEntry entry = new HrcBoneEntry();
                entry.Name = section[0];
                entry.Parent = section[1];
                entry.Length = (float)Convert.ToDecimal(section[2]);
                
                List<string> rsdFilesInfo = section[3].Split(' ').ToList();
                entry.RsdFileCount = Convert.ToInt32(rsdFilesInfo[0]);
                if (entry.RsdFileCount > 0) {
                    entry.RsdFileNames = rsdFilesInfo.GetRange(1, entry.RsdFileCount);
                }
                
                return entry;
            }
        }

        struct HrcFile : IDisposable {
            public int HeaderVersion = 0;
            public string SkeletonName = "";
            public int BoneCount = 0;
            public List<HrcBoneEntry> Bones = new List<HrcBoneEntry>();


            private Stream? _stream;
            private Stream Stream {
                readonly get => _stream ?? throw new Exception("Trying to read from Stream when it is null");
                set => _stream = value;
            }

            public HrcFile() {}

            public static HrcFile FromStream(Stream s) {
                HrcFile file = new HrcFile { Stream = s };
                List<List<string>> sections = file.Stream.ReadAllLines().ToList().SplitBy(line => string.IsNullOrWhiteSpace(line)).ToList();
                
                file.HeaderFromSection(sections[0]);
                foreach (var section in sections.GetRange(1, sections.Count - 1)) {
                    file.Bones.Add(HrcBoneEntry.FromHRCFileSection(section));
                }

                return file;
            }

            private void HeaderFromSection(List<string> section) {
                if (section.Count() != HrcConstants.HeaderSectionSize) {
                    throw new Exception("HRC File Header Section is not a valid size");
                }

                try {
                    HeaderVersion = Convert.ToInt32(section[0].Split(' ')[1]);
                    SkeletonName = section[1].Split(' ')[1];
                    BoneCount = Convert.ToInt32(section[2].Split(' ')[1]);
                } catch (FormatException) {
                    throw new Exception("Failed to parse HRC header values");
                }
            }

            public readonly void Dispose() {
                Stream?.Dispose();
            }
        }
    }
}