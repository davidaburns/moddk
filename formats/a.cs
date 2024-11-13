namespace ModDK {
    namespace FileFormats {
        struct AFile : IDisposable {
            private Stream? _stream;
            private Stream Stream {
                readonly get => _stream ?? throw new Exception("Trying to read from Stream when it is null");
                set => _stream = value;
            }

            public AFile() {}

            public static AFile FromStream(Stream s) {
                AFile file = new AFile { Stream = s };
                return file;
            }

            public readonly void Dispose() {
                Stream?.Dispose();
            }
        }
    }
}