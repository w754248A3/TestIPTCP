﻿using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace LeiKaiFeng.TCPIP
{



    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public readonly struct PseudoHeader
    {
        [FieldOffset(0)]
        readonly IPv4Address _SourceAddress;

        [FieldOffset(4)]
        readonly IPv4Address _DesAddress;

        [FieldOffset(9)]
        readonly byte _Pro;

        [FieldOffset(10)]
        readonly ushort _Length;

        public PseudoHeader(IPv4Address sourceAddress, IPv4Address desAddress, Protocol protocol, ushort length)
        {
            _SourceAddress = sourceAddress;
            _DesAddress = desAddress;
            _Pro = (byte)protocol;
            _Length = Meth.AsBigEndian(length);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct UDPHeader
    {
        public const int HEADER_SIZE = 8;


        [FieldOffset(0)]
        ushort _SourcePort;

        [FieldOffset(2)]
        ushort _DesPort;

        [FieldOffset(4)]
        ushort _Length;

        [FieldOffset(6)]
        ushort _Sum;

        public ushort SourcePort => Meth.AsBigEndian(_SourcePort);


        public ushort DesPort => Meth.AsBigEndian(_DesPort);




        public ushort Length => Meth.AsBigEndian(_Length);


        public ushort Sum => Meth.AsBigEndian(_Sum);


        public ushort GetSubHeaderSizeLength()
        {
            return (ushort)(Length - HEADER_SIZE);
        }


       

        public static ushort GetAllLength(ushort length)
        {
            return (ushort)(length + HEADER_SIZE);
        }

        public static Span<byte> SubHeaderSizeSlice(Span<byte> buffer)
        {
            return buffer.Slice(HEADER_SIZE);
        }   



        public static void Set(
            ref UDPHeader header,
            IPv4Address sourceAddress,
            ushort sourcePort,
            IPv4Address desAddress,
            ushort desPort,
            Span<byte> headerAndData)
        {
            header = new UDPHeader();

            header._SourcePort = Meth.AsBigEndian(sourcePort);

            header._DesPort = Meth.AsBigEndian(desPort);

            header._Length = Meth.AsBigEndian((ushort)headerAndData.Length);

            var per = new PseudoHeader(
                sourceAddress,
                desAddress,
                Protocol.UDP,
                (ushort)headerAndData.Length);


            header._Sum = Meth.AsBigEndian(Meth.CalculationHeaderChecksum(per, headerAndData));

        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly struct IPv4EndPoint : IEquatable<IPv4EndPoint>
    {
        public IPv4EndPoint(IPv4Address address, ushort port)
        {
            Address = address;
            Port = port;
        }



        public IPv4Address Address { get; }

        public ushort Port { get; }

        public bool Equals(IPv4EndPoint other)
        {
            return (Address, Port).Equals((other.Address, other.Port));
        }

        public override bool Equals(object obj)
        {
            if (obj is IPv4EndPoint value)
            {
                return this.Equals(value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Address, Port).GetHashCode();
        }

        public override string ToString()
        {
            return $"{Address}:{Port}";
        }
    }


    [StructLayout(LayoutKind.Auto)]
    public readonly struct Quaternion : IEquatable<Quaternion>
    {
        public Quaternion(IPv4EndPoint source, IPv4EndPoint des)
        {
            Source = source;
            Des = des;
        }

        public IPv4EndPoint Source { get; }

        public IPv4EndPoint Des { get; }

        public Quaternion Reverse()
        {
            return new Quaternion(Des, Source);
        }

        public bool Equals(Quaternion other)
        {
            return (Source, Des).Equals((other.Source, other.Des));
        }

        public override bool Equals(object obj)
        {
            if (obj is Quaternion value)
            {
                return this.Equals(value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Source, Des).GetHashCode();
        }

        public override string ToString()
        {
            return $"{Source} {Des}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public readonly struct IPv4Address : IEquatable<IPv4Address>
    {
        [FieldOffset(0)]
        readonly uint _Value;

        [FieldOffset(0)]
        readonly byte _v0;

        [FieldOffset(1)]
        readonly byte _v1;

        [FieldOffset(2)]
        readonly byte _v2;

        [FieldOffset(3)]
        readonly byte _v3;

        public IPv4Address(byte v0, byte v1, byte v2, byte v3) : this()
        {

            _v0 = v0;
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
        }



        public IPAddress GetIPAddress()
        {
            return IPAddress.Parse(this.ToString());
        }


        public bool Equals(IPv4Address other)
        {
            return this._Value == other._Value;
        }

        public override bool Equals(object obj)
        {

            if (obj is IPv4Address address)
            {
                return this.Equals(address);
            }
            else
            {
                return false;
            }

        }


        public override int GetHashCode()
        {
            return (int)_Value;
        }

        public override string ToString()
        {
            return $"{_v0}.{_v1}.{_v2}.{_v3}";
        }
    }


    [StructLayout(LayoutKind.Explicit, Size = 4)]
    struct IPHeaderBit64_96
    {
        [FieldOffset(0)]
        public byte _TTL;

        [FieldOffset(1)]
        public byte _Protocol;

        [FieldOffset(2)]
        public ushort _HeaderChecksum;
    }


    [StructLayout(LayoutKind.Explicit, Size = 4)]
    struct IPHeaderBit32_64
    {
        [FieldOffset(0)]
        public ushort _16Flag;

        [FieldOffset(2)]
        public ushort _Offset;

        [FieldOffset(2)]
        public byte _Frag;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    struct IPHeaderBit0_32
    {
        [FieldOffset(0)]
        public byte _Version_HeaderLength;

        [FieldOffset(1)]
        byte _TOS;


        //576
        [FieldOffset(2)]
        public ushort _AllLegnth;


    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    internal struct IPHeader
    {
        public const int HEADER_SIZE = 20;

        [FieldOffset(0)]
        IPHeaderBit0_32 _IPHeader0_32;


        [FieldOffset(4)]
        IPHeaderBit32_64 _IPHeader32_64;


        [FieldOffset(8)]
        IPHeaderBit64_96 _IPHeader64_96;


        [FieldOffset(12)]
        IPv4Address _SourceAddress;

        [FieldOffset(16)]
        IPv4Address _DesAddress;

        public static void Set(
            ref IPHeader header,
            IPv4Address sourceAddress,
            IPv4Address desAddress,
            Protocol protocol)
        {

            header._SourceAddress = sourceAddress;

            header._DesAddress = desAddress;


            header._IPHeader64_96._Protocol = (byte)protocol;

            header.CalculationHeaderChecksum();
        }

        public static void Set(ref IPHeader header, IPData ipData, ushort count)
        {
            header = new IPHeader();

            ushort allLength = GetAllLength(count);

            header._IPHeader0_32._Version_HeaderLength = 0b0100_0101;


            header._IPHeader0_32._AllLegnth = Meth.AsBigEndian(allLength);

            header._IPHeader64_96._Protocol = (byte)ipData.Protocol;

            header._IPHeader64_96._TTL = 128;

            header._SourceAddress = ipData.SourceAddress;

            header._DesAddress = ipData.DesAddress;

            header._IPHeader32_64._16Flag = Meth.GetCount();

            header._IPHeader32_64._Frag = 0x40;


            header.CalculationHeaderChecksum();
        }

        

        public void CalculationHeaderChecksum()
        {
            _IPHeader64_96._HeaderChecksum = 0;

            _IPHeader64_96._HeaderChecksum = Meth.AsBigEndian(Meth.CalculationHeaderChecksum(ref this));
        }

        public static Span<byte> SubHeaderSizeSlice(Span<byte> buffer)
        {
            return buffer.Slice(HEADER_SIZE);
        }

        public static ushort GetAllLength(ushort length)
        {
            return (ushort)(length + HEADER_SIZE);
        }

        public byte Version => (byte)(_IPHeader0_32._Version_HeaderLength >> 4);

        public byte HeadLength => (byte)(_IPHeader0_32._Version_HeaderLength & 0x0F);

        public ushort AllLength => Meth.AsBigEndian(_IPHeader0_32._AllLegnth);

        public ushort HeaderChecksum => Meth.CalculationHeaderChecksum(ref this);

        public byte TTL => _IPHeader64_96._TTL;

        public Protocol Protocol => (Protocol)_IPHeader64_96._Protocol;

        public IPv4Address SourceAddress => _SourceAddress;

        public IPv4Address DesAddress => _DesAddress;
    }



    public enum Protocol : byte
    {
        TCP = 6,

        UDP = 17,
    }

}
