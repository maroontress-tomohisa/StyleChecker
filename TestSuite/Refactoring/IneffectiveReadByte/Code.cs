#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.IneffectiveReadByte
{
    using System;
    using System.IO;

    public sealed class Code
    {
        public byte[] byteArray;
        public byte[] ByteArrayProperty { get; private set; } = new byte[1];
        public BinaryReader binaryReader;
        public BinaryReader BinaryReaderProperty { get; set; }

        public void Field()
        {
            for (int i = 0; i < 10; ++i)
//@         ^binaryReader byteArray
            {
                byteArray[i] = binaryReader.ReadByte();
            }
        }

        public void Local()
        {
            var stream = new MemoryStream();
            var reader = new BinaryReader(stream);
            var buffer = new byte[4];
            for (var i = 0; i < 4; ++i)
//@         ^reader buffer
            {
                buffer[i] = reader.ReadByte();
            }
        }

        public void AutoProperty()
        {
            for (int i = 0; i < 1; ++i)
//@         ^BinaryReaderProperty ByteArrayProperty
            {
                ByteArrayProperty[i] = BinaryReaderProperty.ReadByte();
            }
        }

        public void Parameter(byte[] array, BinaryReader reader)
        {
            for (int i = 0; i < 1; ++i)
//@         ^reader array
            {
                array[i] = reader.ReadByte();
            }
        }

        public void NumericLiteral(byte[] array, BinaryReader reader)
        {
            for (int i = 100_000; i < 100_100; ++i)
//@         ^reader array
            {
                array[i] = reader.ReadByte();
            }
        }

        public void InNestedBlock()
        {
            var stream = new MemoryStream();
            var reader = new BinaryReader(stream);
            var buffer = new byte[4];
            for (var i = 0; i < 4; ++i)
//@         ^reader buffer
            {
                {
                    buffer[i] = reader.ReadByte();
                }
            }
        }

        public void SystemInt32(BinaryReader reader, byte[] buffer)
        {
            for (System.Int32 i = 0; i < 10; ++i)
//@         ^reader buffer
            {
                buffer[i] = reader.ReadByte();
            }
        }

        public void Int32(BinaryReader reader, byte[] buffer)
        {
            for (Int32 i = 0; i < 10; ++i)
//@         ^reader buffer
            {
                buffer[i] = reader.ReadByte();
            }
        }

        public void LessThanWithOneIteration(
            BinaryReader reader, byte[] buffer)
        {
            for (var i = 5; i < 6; ++i)
//@         ^reader buffer
            {
                buffer[i] = reader.ReadByte();
            }
        }

        public void LessThanOrEqualWithSameStartAndEnd(
            BinaryReader reader, byte[] buffer)
        {
            for (var i = 5; i <= 5; i++)
//@         ^reader buffer
            {
                buffer[i] = reader.ReadByte();
            }
        }
    }
}
