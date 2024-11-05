namespace ModDK {
    namespace FileFormats {
        struct PHeader {
            public int Offset00;
            public int Offset04;
            public int VertexColor;
            public int NumberOfVertecies;
            public int NumberOfNormals;
            public int Offset14;
            public int NumberOfTexCs;
            public int NumberOfEdges;
            public int NumberOfPolygons;
            public int Offset28;
            public int Offset2C;
            public int MirexH;
            public int NumberOfGroups;
            public int MirexG;
            public int Offset3C;
            public int[] Unknown = new int[16];

            public PHeader() {}
        }

        struct PVertex {
            public float X;
            public float Y;
            public float Z;
        }

        struct PTextureCoordinate {
            public float X;
            public float Y;
        }

        struct PColor {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Reserved;
        }

        struct PEdge {
            public short[] Vertex = new short[2];

            public PEdge() {}
        }

        struct PPolygon {
            public short Tag1;
            public short[] Vertex = new short[3];
            public short[] Normal = new short[3];
            public short[] Edge = new short[3];
            public int Tag2;

            public PPolygon() {}
        }

        struct PHundretsChunk {
            public int a;
            public int b;
            public byte c;
            public byte d;
            public short e;
            public byte f;
            public byte g;
            public short h;
        }
        
        struct PFile : IDisposable {
            private Stream? _stream;
            private Stream Stream {
                readonly get => _stream ?? throw new Exception("Trying to read from Stream when it is null");
                set => _stream = value;
            }

            public PFile() {}

            public static PFile FromStream(Stream s) {
                PFile file = new PFile { Stream = s };
                return file;
            }

            public readonly void Dispose() {
                Stream?.Dispose();
            }
        }
    }
}