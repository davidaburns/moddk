using ModDK.Extensions;

namespace ModDK {
    namespace FileFormats {
        struct RsdFileConstants {
            public const string Header = "@RSD940102";
            public const string PolygonMeshFile = "PLY";
            public const string MaterialFile = "MAT";
            public const string GroupFile = "GRP";
            public const string TextureCount = "NTEX";
            public const string TextureFile = "TEX[";
            public const char CommentIndicator = '#';
        }

        struct RsdFile : IDisposable {
            public string Header = "";
            public string PolygonMeshFilename = "";
            public string MaterialFilename = "";
            public string GroupFilename = "";
            public int TextureCount = 0;
            public List<string> TextureFilenames = new List<string>();

            private Stream? _stream;
            private Stream Stream {
                readonly get => _stream ?? throw new Exception("Trying to read from Stream when it is null");
                set => _stream = value;
            }

            public RsdFile() {}

            public static RsdFile FromStream(Stream s) {
                RsdFile file = new RsdFile { Stream = s };
                List<string> contents = s.ReadAllLines().Where(line => line[0] != RsdFileConstants.CommentIndicator && !string.IsNullOrWhiteSpace(line)).ToList();

                if (contents.Count() == 0) {
                    throw new Exception("Contents of RSD file is empty");
                }

                file.Header = contents[0];
                if (file.Header != RsdFileConstants.Header) {
                    throw new Exception($"Invalid RSD file header: {file.Header}");
                }

                foreach (string line in contents.GetRange(1, contents.Count()-1)) {
                    List<string> property = line.Split('=').ToList();
                    if (property.Count() != 2) {
                        throw new Exception($"Invalid RSD file property: {line}");
                    }

                    string type = property[0];
                    string value = property[1];

                    if (type.Contains(RsdFileConstants.TextureFile)) {
                        file.TextureFilenames.Add(value);
                    } else {
                        switch (type) {
                            case RsdFileConstants.PolygonMeshFile: file.PolygonMeshFilename=value; break;
                            case RsdFileConstants.MaterialFile: file.MaterialFilename=value; break;
                            case RsdFileConstants.GroupFile: file.GroupFilename=value; break;
                            case RsdFileConstants.TextureCount: file.TextureCount = Convert.ToInt32(value); break;
                            default: 
                                throw new Exception($"Unknown RSD file property: {type}");
                        }
                    }
                }

                return file;
            }

            public readonly void Dispose() {
                Stream?.Dispose();
            }
        }
    }
}