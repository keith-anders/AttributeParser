using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace AttributeCloner
{
    internal class BlobReader : BinaryReader
    {
        internal const byte FIELD = 0x53;
        internal const byte PROPERTY = 0x54;

        private static readonly Type[] s_primitives = new Type[]
        {
            null,
            null,
            typeof(Boolean),
            typeof(Char),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(String)
        };

        private const byte ELEMENT_TYPE_ENUM = 0x55;
        private const byte ELEMENT_TYPE_ARRAY = 0x1d;
        private const byte ELEMENT_TYPE_OBJECT = 0x51;

        private Assembly _defaultAssembly;
        private Type GetTypeFromString(string typeName) => Type.GetType(typeName) ?? _defaultAssembly.GetType(typeName);

        internal BlobReader(byte[] blob, Assembly defaultAssembly)
            : base(new MemoryStream(blob), Encoding.Unicode, leaveOpen: false)
        {
            _defaultAssembly = defaultAssembly;
        }

        private byte? Head
        {
            get
            {
                int value = BaseStream.ReadByte();
                if (value == -1)
                    return null;
                BaseStream.Seek(-1, SeekOrigin.Current);
                return (byte)value;
            }
        }

        internal bool Completed { get => Head == null; }

        internal object ReadFixedArgOfType(Type type)
        {
            switch (type.Name)
            {
                case nameof(Boolean): return ReadBoolean();
                case nameof(Char): return ReadChar();
                case nameof(SByte): return ReadSByte();
                case nameof(Byte): return ReadByte();
                case nameof(Int16): return ReadInt16();
                case nameof(UInt16): return ReadUInt16();
                case nameof(Int32): return ReadInt32();
                case nameof(UInt32): return ReadUInt32();
                case nameof(Int64): return ReadInt64();
                case nameof(UInt64): return ReadUInt64();
                case nameof(Single): return ReadSingle();
                case nameof(Double): return ReadDouble();
                case nameof(String): return ReadSerString();
                case nameof(Type): return ReadType();
                case nameof(Object): return ReadObject();
                default:
                    if (type.IsEnum)
                        return Enum.ToObject(type, ReadFixedArgOfType(Enum.GetUnderlyingType(type)));
                    if (type.IsArray)
                    {
                        Type elementType = type.GetElementType();
                        uint sizeOfArray = ReadUInt32();
                        if (sizeOfArray == 0xFFFFFFFF)
                            return null;
                        Array arr = Array.CreateInstance(elementType, sizeOfArray);
                        for (uint i = 0; i < sizeOfArray; ++i)
                            arr.SetValue(ReadFixedArgOfType(elementType), i);
                        return arr;
                    }
                    throw new ArgumentException($"Unexpected type {type.FullName} in attribute ctor argument.");
            }
        }

        private object ReadObject() => ReadFixedArgOfType(ReadFieldOrPropType());
        private Type ReadType() => GetTypeFromString(ReadSerString());

        internal string ReadSerString()
        {
            byte head = Head.Value;
            if (head == 0xFF)
            {
                ReadByte();
                return null;
            }
            int length = ReadPackedLength();

            if (length == 0)
                return String.Empty;
            if (length == -1)
                return null;

            return Encoding.UTF8.GetString(ReadBytes(length), 0, length);
        }

        internal int ReadPackedLength()
        {
            // Storage mechanism for PackedLen:
            // if value is 0-127: store it in the 7 LSB of a single byte and set the one MSB to 0.
            // if value is 128-0x3fff: store it in the 14 LSB of a 16-bit word and set the two MSB to 10.
            // if value is 0x4000-0x1fffffff: store it in the 29 LSB of a 32-bit word and set the three MSB to 110.
            byte head = ReadByte();
            if (head == 0xFF)
                return -1;
            if (head < 128)
                return head;
            if (head < 192)
                return BitConverter.ToInt32(new byte[]
                {
                    (byte)(head - 128),
                    ReadByte()
                }, 0);
            return BitConverter.ToInt32(new byte[]
            {
                (byte)(head - 192),
                ReadByte(),
                ReadByte(),
                ReadByte()
            }, 0);
        }
        
        internal Type ReadFieldOrPropType()
        {
            int typeByte = ReadByte();
            if (typeByte < s_primitives.Length)
                return s_primitives[typeByte];
            switch (typeByte)
            {
                case ELEMENT_TYPE_ENUM: return GetTypeFromString(ReadSerString());
                case ELEMENT_TYPE_OBJECT: return typeof(Object);
                case ELEMENT_TYPE_ARRAY: return ReadFieldOrPropType().MakeArrayType();
                default: throw new ArgumentException($"Unexpected token at index {BaseStream.Position - 1}: should have been FieldOrPropType.");
            }
        }
    }
}
