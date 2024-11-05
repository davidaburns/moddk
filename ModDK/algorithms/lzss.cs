namespace ModDK {
    namespace Algorithms {
        struct LzssConstants {
            public const int MinimumReferenceLength = 3;
            public const int MaximumReferenceLength = 18;
            public const byte LeftNibbleMask = 0b11110000;
            public const byte RightNibbleMask = 0b00001111;
            public const short WindowMask = 0x0FFF;
            public const short WindowSize = 0x1000;
            public const int BitsPerByte = 8;
        }

        class LzssCompression {}
    }
}