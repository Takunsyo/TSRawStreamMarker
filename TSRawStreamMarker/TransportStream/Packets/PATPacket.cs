using System.Collections.Generic;

namespace TSRawStreamMarker.TransportStream.Packets
{
    /// <summary>
    /// Program Association Table(PAT)
    /// <para>The Program Association Table provides the correspondence between a program_number and the PID 
    /// value of the Transport Stream packets which carry the program definition.</para>
    /// </summary>
    public class PATPacket : IPSISection
    {
        public bool HasPointer { get; private set; }
        /// <summary>
        /// Program specific information pointer.
        /// <para>This field will present when <see cref="TSPacket.IsPayloadEntry"/> is set to true.</para>
        /// </summary>
        public byte Pointer_Field
        {
            get=> this.HasPointer ? this.Data.ReadByte(0, 8) : (byte)0;
            set
            {
                if (this.HasPointer)
                    this.Data.WriteByte(value, 0, 8);
            }
        }
        /// <summary>
        /// Table ID
        /// </summary>
        public byte TableID
        {
            get=> this.Data.ReadByte(0 + (this.HasPointer ? 8 : 0), 8);
            set=>this.Data.WriteByte(value, 0 + (this.HasPointer ? 8 : 0), 8);
        }

        /// <summary>
        /// This should be set to true.
        /// </summary>
        public bool SyntaxIndicator
        {
            get=> this.Data.ReadBool(8 + (this.HasPointer ? 8 : 0));
            set=>this.Data.WriteBool(value,8 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// Is allways false in <see cref="PATPacket"/>.
        /// </summary>
        public bool IsPrivate
        {
            get=> this.Data.ReadBool(9+(this.HasPointer? 8:0));
            set => this.Data.WriteBool(value, 9 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// Reserved.
        /// </summary>
        public byte PSIReserved
        {
            get=>this.Data.ReadByte(10 + (this.HasPointer ? 8 : 0), 2);
            set=>this.Data.WriteByte(value, 10 + (this.HasPointer ? 8 : 0), 2);
        }
        /// <summary>
        /// Specify the number of bytes of the section, starting immediately following 
        /// the SectionLength field, and including the CRC. the Value should never be greater then 0x3FD.
        /// </summary>
        public int SectionLength
        {// The sectionLength's first 2 bits should allways be '00'
            get =>this.Data.ReadInt(14 + (this.HasPointer ? 8 : 0), 10);
            set=>this.Data.WriteInt(value, 14 + (this.HasPointer ? 8 : 0), 10);
        }
        /// <summary>
        /// This value serves as a label to identify this TS from any other muliplex within a network.
        /// </summary>
        public int TransPostStreamID
        {
            get=>this.Data.ReadInt(24 + (this.HasPointer ? 8 : 0), 16);
            set=>this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0), 16);
        }
        private byte Reserved
        {
            get=> this.Data.ReadByte(40 + (this.HasPointer ? 8 : 0), 2);
            set=>this.Data.WriteByte(value, 40 + (this.HasPointer ? 8 : 0), 2);
        }
        /// <summary>
        /// The value shall be incremented by 1 modulo 32 whenever the definition of the PAT changes.
        /// </summary>
        public byte Version
        {
            get=>this.Data.ReadByte(42 + (this.HasPointer ? 8 : 0), 5);
            set=>this.Data.WriteByte(value, 42+ (this.HasPointer? 8 : 0), 5);
        }
        /// <summary>
        /// Current Next Indicator. indicates that the PAT is currently applicable.
        /// </summary>
        public bool IsApplicable
        {
            get => this.Data.ReadBool(47 + (this.HasPointer ? 8 : 0));
            set => this.Data.WriteBool(value, 47 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// The section number of the first section in the PAT shall be 0x00 it 
        /// shall be incremented by 1 with each additional section in the PAT.
        /// </summary>
        public byte SectionNumber
        {
            get => this.Data.ReadByte(48 + (this.HasPointer ? 8 : 0), 8);
            set => this.Data.WriteByte(value, 48 + (this.HasPointer ? 8 : 0), 8);
        }
        /// <summary>
        /// The number of the laster section.
        /// </summary>
        public byte LastSectionNumber
        {
            get => this.Data.ReadByte(56 + (this.HasPointer ? 8 : 0), 8);
            set => this.Data.WriteByte(value, 56 + (this.HasPointer ? 8 : 0), 8);
        }

        /// <summary>
        /// The program specifation structure.
        /// </summary>
        public struct Program
        {
            /// <summary>
            /// The program number specifies thr program to which the program map PID is applicable.
            /// When set to 0x0000, then the following PID refrence shall be the network PID. 
            /// For all other cases the value of this field is user defined.
            /// </summary>
            public int ProgramNumber
            {
                get => this.Data.ReadInt(0, 16);
                set => this.Data.WriteInt(value, 0, 16);
            }
            /// <summary>
            /// present when <see cref="ProgramNumber"/> equals 0.
            /// Specifies the PID of the TS packets which shall contain the Network Information Table.
            /// This value is user defined.
            /// <para>when <see cref="ProgramNumber"/> is not 0. This value specifying the PID of the TS 
            /// packets program number shall have more then one Program Map PID assignment.</para>
            /// </summary>
            public int PID
            {
                get=>this.Data.ReadInt(19, 13);
                set=>this.Data.WriteInt(value, 19, 13);
            }
            /// <summary>
            /// present 
            /// <para>when <see cref="ProgramNumber"/> is not 0. This value specifying the PID of the TS packets program number shall have more then one Program Map PID assignment.</para>
            /// </summary>

            private BitPacket Data { get; set; }

            public Program(BitPacket packet)
            {
                this.Data = packet;
                //this.ProgramNumber = packet.ReadInt(16);
                //packet.SkipBit(3); // reserved.
                //if(this.ProgramNumber == 0)
                //{
                //    this.NetworkID = packet.ReadInt(13);
                //    this.ProgramMapID = 0;
                //}
                //else
                //{
                //    this.ProgramMapID = packet.ReadInt(13);
                //    this.NetworkID = 0;
                //}
            }
            public Program(byte[] packet)
            {
                this.Data = new BitPacket(packet);
            }

            public byte[] GetBytes() => this.Data.ToByteArray();
        }

        private List<Program> _Programs;
        /// <summary>
        /// The program information on this PAT.
        /// </summary>
        public List<Program> Programs
        {
            get
            {
                if(_Programs is null)
                {
                    int offset = 64 + (this.HasPointer ? 8 : 0);
                    var counter = 0;
                    _Programs = new List<Program>();
                    while (counter < this.SectionLength - 9)
                    {
                        _Programs.Add(new Program(this.Data.ReadBlock(offset, 32)));
                        counter += 4;
                        offset += 32;
                    }
                }
                return _Programs;
            }
            set
            {
                if(_Programs != value) { 
                    _Programs = value;
                    int offset = 64 + (this.HasPointer ? 8 : 0);
                    this.SectionLength = 4 + 5 + value.Count * 4;
                    foreach(var i in value)
                    {
                        this.Data.WriteBlock(i.GetBytes(), offset,32);
                        offset += 32;
                    }
                    //Rewrite CRC32//
                }
            }
        }

        public uint CRC32
        {
            get => this.Data.ReadUInt(24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
            set => this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
        }

        public BitPacket Data { get; set; }

        public PATPacket(BitPacket packet,bool hasPointer)
        {
            this.HasPointer = hasPointer;
            this.Data = packet;
        }

        public PATPacket(byte[] packet, bool hasPointer)
        {
            this.HasPointer = hasPointer;
            this.Data = new BitPacket(packet);
        }
    }
}
