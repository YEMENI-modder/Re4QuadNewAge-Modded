using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleEndianBinaryIO
{
    public static class EndianBitConverter
    {
        public static Endianness NativeEndianness { get => BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian; }

        public static byte[] GetBytes(short value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(value));
            }
        }
        public static byte[] GetBytes(ushort value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(value));
            }
        }
        public static byte[] GetBytes(int value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(value));
            }
        }
        public static byte[] GetBytes(uint value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(value));
            }
        }
        public static byte[] GetBytes(long value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(value));
            }
        }
        public static byte[] GetBytes(ulong value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(value));
            }
        }
        public static byte[] GetBytes(float value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0)));
            }
        }
        public static byte[] GetBytes(double value, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.GetBytes(value);
            }
            else
            {
                return BitConverter.GetBytes(Reverse(BitConverter.ToUInt64(BitConverter.GetBytes(value), 0)));
            }
        }
        public static double ToDouble(byte[] value, int startIndex, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToDouble(value, startIndex);
            }
            else
            {
                return BitConverter.ToDouble(BitConverter.GetBytes(Reverse(BitConverter.ToUInt64(value, startIndex))), 0);
            }
        }
        public static float ToSingle(byte[] value, int startIndex, Endianness endianness)
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToSingle(value, startIndex);
            }
            else
            {
                return BitConverter.ToSingle(BitConverter.GetBytes(Reverse(BitConverter.ToUInt32(value, startIndex))), 0);
            }
        }
        public static short ToInt16(byte[] value, int startIndex, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToInt16(value, startIndex);
            }
            else
            {
                return Reverse(BitConverter.ToInt16(value, startIndex));
            }
        }
        public static ushort ToUInt16(byte[] value, int startIndex, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToUInt16(value, startIndex);
            }
            else
            {
                return Reverse(BitConverter.ToUInt16(value, startIndex));
            }
        }
        public static int ToInt32(byte[] value, int startIndex, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToInt32(value, startIndex);
            }
            else
            {
                return Reverse(BitConverter.ToInt32(value, startIndex));
            }
        }
        public static uint ToUInt32(byte[] value, int startIndex, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToUInt32(value, startIndex);
            }
            else
            {
                return Reverse(BitConverter.ToUInt32(value, startIndex));
            }
        }
        public static long ToInt64(byte[] value, int startIndex, Endianness endianness) 
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToInt64(value, startIndex);
            }
            else
            {
                return Reverse(BitConverter.ToInt64(value, startIndex));
            }
        }
        public static ulong ToUInt64(byte[] value, int startIndex, Endianness endianness)
        {
            if (endianness == NativeEndianness)
            {
                return BitConverter.ToUInt64(value, startIndex);
            }
            else
            {
                return Reverse(BitConverter.ToUInt64(value, startIndex));
            }
        }

        public static short Reverse(short value)
        {
            return (short)(
                (((ushort)value & 0xFF00) >> 8) |
                (((ushort)value & 0x00FF) << 8) );
        }

        public static ushort Reverse(ushort value)
        {
            return (ushort)(
                ((value & 0xFF00) >> 8) |
                ((value & 0x00FF) << 8) );
        }

        public static int Reverse(int value)
        {
            return (int)(
                (((uint)value & 0xFF000000) >> 24) |
                (((uint)value & 0x00FF0000) >> 08) |
                (((uint)value & 0x0000FF00) << 08) |
                (((uint)value & 0x000000FF) << 24) );
        }

        public static uint Reverse(uint value)
        {
            return (uint)(
                ((value & 0xFF000000) >> 24) |
                ((value & 0x00FF0000) >> 08) |
                ((value & 0x0000FF00) << 08) |
                ((value & 0x000000FF) << 24) );
        }

        public static long Reverse(long value)
        {
            return (long)(
                (((ulong)value & 0xFF00000000000000UL) >> 56) |
                (((ulong)value & 0x00FF000000000000UL) >> 40) |
                (((ulong)value & 0x0000FF0000000000UL) >> 24) |
                (((ulong)value & 0x000000FF00000000UL) >> 08) |
                (((ulong)value & 0x00000000FF000000UL) << 08) |
                (((ulong)value & 0x0000000000FF0000UL) << 24) |
                (((ulong)value & 0x000000000000FF00UL) << 40) |
                (((ulong)value & 0x00000000000000FFUL) << 56) );
        }

        public static ulong Reverse(ulong value)
        {
            return (ulong)(
                ((value & 0xFF00000000000000UL) >> 56) |
                ((value & 0x00FF000000000000UL) >> 40) |
                ((value & 0x0000FF0000000000UL) >> 24) |
                ((value & 0x000000FF00000000UL) >> 08) |
                ((value & 0x00000000FF000000UL) << 08) |
                ((value & 0x0000000000FF0000UL) << 24) |
                ((value & 0x000000000000FF00UL) << 40) |
                ((value & 0x00000000000000FFUL) << 56) );
        }

        // --- Allocation-free WriteTo: writes value directly into dest[offset..] ---
        public static void WriteTo(short value, byte[] dest, int offset, Endianness endianness)
        {
            if (endianness == NativeEndianness) { dest[offset] = (byte)value; dest[offset + 1] = (byte)(value >> 8); }
            else { dest[offset] = (byte)(value >> 8); dest[offset + 1] = (byte)value; }
        }
        public static void WriteTo(ushort value, byte[] dest, int offset, Endianness endianness)
        {
            if (endianness == NativeEndianness) { dest[offset] = (byte)value; dest[offset + 1] = (byte)(value >> 8); }
            else { dest[offset] = (byte)(value >> 8); dest[offset + 1] = (byte)value; }
        }
        public static void WriteTo(int value, byte[] dest, int offset, Endianness endianness)
        {
            if (endianness == NativeEndianness)
            {
                dest[offset] = (byte)value;
                dest[offset + 1] = (byte)(value >> 8);
                dest[offset + 2] = (byte)(value >> 16);
                dest[offset + 3] = (byte)(value >> 24);
            }
            else
            {
                dest[offset] = (byte)(value >> 24);
                dest[offset + 1] = (byte)(value >> 16);
                dest[offset + 2] = (byte)(value >> 8);
                dest[offset + 3] = (byte)value;
            }
        }
        public static void WriteTo(uint value, byte[] dest, int offset, Endianness endianness)
        {
            if (endianness == NativeEndianness)
            {
                dest[offset] = (byte)value;
                dest[offset + 1] = (byte)(value >> 8);
                dest[offset + 2] = (byte)(value >> 16);
                dest[offset + 3] = (byte)(value >> 24);
            }
            else
            {
                dest[offset] = (byte)(value >> 24);
                dest[offset + 1] = (byte)(value >> 16);
                dest[offset + 2] = (byte)(value >> 8);
                dest[offset + 3] = (byte)value;
            }
        }
        public static void WriteTo(float value, byte[] dest, int offset, Endianness endianness)
        {
            uint bits = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            WriteTo(bits, dest, offset, endianness);
        }
        public static void WriteTo(double value, byte[] dest, int offset, Endianness endianness)
        {
            ulong bits = BitConverter.ToUInt64(BitConverter.GetBytes(value), 0);
            WriteTo(bits, dest, offset, endianness);
        }
        public static void WriteTo(long value, byte[] dest, int offset, Endianness endianness)
        {
            if (endianness == NativeEndianness)
            {
                for (int i = 0; i < 8; i++) dest[offset + i] = (byte)(value >> (i * 8));
            }
            else
            {
                for (int i = 0; i < 8; i++) dest[offset + 7 - i] = (byte)(value >> (i * 8));
            }
        }
        public static void WriteTo(ulong value, byte[] dest, int offset, Endianness endianness)
        {
            if (endianness == NativeEndianness)
            {
                for (int i = 0; i < 8; i++) dest[offset + i] = (byte)(value >> (i * 8));
            }
            else
            {
                for (int i = 0; i < 8; i++) dest[offset + 7 - i] = (byte)(value >> (i * 8));
            }
        }
    }
}
