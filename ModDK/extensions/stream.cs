using System.Text;

namespace ModDK {
    namespace Extensions {
        public static class StreamExtensions {
            public static byte[] ReadBytes(this Stream s, int count) {
                byte[] buffer = new byte[count];
                s.Read(buffer, 0, count);

                return buffer;
            }

            public static byte ReadByte(this Stream s) {
                return (byte)s.ReadByte();
            }

            public static short ReadInt16(this Stream s) {                
                return BitConverter.ToInt16(s.ReadBytes(2), 0);
            }

            public static ushort ReadUInt16(this Stream s) {
                return BitConverter.ToUInt16(s.ReadBytes(2), 0);
            }

            public static int ReadInt32(this Stream s) {
                return BitConverter.ToInt32(s.ReadBytes(4), 0);
            }

            public static uint ReadUInt32(this Stream s) {
                return BitConverter.ToUInt32(s.ReadBytes(4), 0);
            }

            public static long ReadInt64(this Stream s) {
                return BitConverter.ToInt64(s.ReadBytes(8), 0);
            }

            public static ulong ReadUInt64(this Stream s) {
                return BitConverter.ToUInt64(s.ReadBytes(8), 0);
            }

            public static IEnumerable<string> ReadLines(this Stream s, Encoding? encoding = null) {
                encoding ??= Encoding.UTF8;
                using (var reader = new StreamReader(s, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true)) {
                    string? line;
                    while ((line = reader?.ReadLine()) != null) {
                        yield return line;
                    }
                }
            }

            public static List<string> ReadAllLines(this Stream s, Encoding? encoding = null) {
                return s.ReadLines(encoding).ToList();
            }
        }
    }
}