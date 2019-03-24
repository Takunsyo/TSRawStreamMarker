using System.Collections.Generic;
using TSRawStreamMarker.TransportStream.Packets.Descriptors;

namespace TSRawStreamMarker.TransportStream.Packets
{
    /// <summary>
    /// Transport Stream Program Map Table(PMT)
    /// <para>The Program Map Table provides the mappings 
    /// between program numbers and the program elements that comprise them.</para>
    /// <para>See (ISO/IEC 13818-1) 2.4.4.8 </para>
    /// </summary>
    public class PMTPacket : IPSISection
    {
        public bool HasPointer { get; private set; }
        /// <summary>
        /// Program specific information pointer.
        /// <para>This field will present when <see cref="TSPacket.IsPayloadEntry"/> is set to true.</para>
        /// </summary>
        public byte Pointer_Field
        {
            get => this.HasPointer ? this.Data.ReadByte(0, 8) : (byte)0;
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
            get => this.Data.ReadByte(0 + (this.HasPointer ? 8 : 0), 8);
            set => this.Data.WriteByte(value, 0 + (this.HasPointer ? 8 : 0), 8);
        }
        /// <summary>
        /// This should be set to true.
        /// </summary>
        public bool SyntaxIndicator
        {
            get => this.Data.ReadBool(8 + (this.HasPointer ? 8 : 0));
            set => this.Data.WriteBool(value, 8 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// Is allways false in <see cref="PATPacket"/>.
        /// </summary>
        public bool IsPrivate
        {
            get => this.Data.ReadBool(9 + (this.HasPointer ? 8 : 0));
            set => this.Data.WriteBool(value, 9 + (this.HasPointer ? 8 : 0));
        }
        /// <summary>
        /// Reserved.
        /// </summary>
        public byte PSIReserved
        {
            get => this.Data.ReadByte(10 + (this.HasPointer ? 8 : 0), 2);
            set => this.Data.WriteByte(value, 10 + (this.HasPointer ? 8 : 0), 2);
        }
        /// <summary>
        /// Specify the number of bytes of the section, starting immediately following 
        /// the SectionLength field, and including the CRC. the Value should never be greater then 0x3FD.
        /// </summary>
        public int SectionLength
        {// The sectionLength's first 2 bits should allways be '00'
            get => this.Data.ReadInt(14 + (this.HasPointer ? 8 : 0), 10);
            set => this.Data.WriteInt(value, 14 + (this.HasPointer ? 8 : 0), 10);
        }

        public int ProgramNumber
        {
            get => this.Data.ReadInt(24 + (this.HasPointer ? 8 : 0), 16);
            set => this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0), 16);
        }
        /// <summary>
        /// The value shall be incremented by 1 modulo 32 whenever the definition of the PAT changes.
        /// </summary>
        public byte Version
        {
            get => this.Data.ReadByte(42 + (this.HasPointer ? 8 : 0), 5);
            set => this.Data.WriteByte(value, 42 + (this.HasPointer ? 8 : 0), 5);
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
        /// The 13-bit PID is the PID of the Transport Stream packets which shall
        /// contain the PCR fields valid for the program specified by program_number.
        /// </summary>
        public int PCR_PID
        {
            get => this.Data.ReadInt(67 + (this.HasPointer ? 8 : 0), 13);
            set => this.Data.WriteInt(value, 67 + (this.HasPointer ? 8 : 0), 13);
        }
        /// <summary>
        /// The lenth of <see cref="ProgramDescriptions"/> in byte. 
        /// 12-bits, first two bits allways be '00'.
        /// </summary>
        public int ProgramInfoLength
        {
            get => this.Data.ReadInt(86 + (this.HasPointer ? 8 : 0), 10);
            set => this.Data.WriteInt(value, 86 + (this.HasPointer ? 8 : 0), 10);
        }

        private List<IDescriptor> _ProgramDescriptions;
        public List<IDescriptor> ProgramDescriptions
        {
            get
            {
                //5byte...+ CRC 4byte total len = seclen-9
                if (_ProgramDescriptions != null) return _ProgramDescriptions;
                var offset = 96 + (this.HasPointer ? 8 : 0);
                var counter = 0;
                _ProgramDescriptions = new List<IDescriptor>();
                var packet = new BitPacket(this.Data.ReadBlock(offset, ProgramInfoLength * 8));
                while (counter < this.ProgramInfoLength)
                {
                    var tmp = new Descriptor(packet);
                    counter += (tmp.Length + 2);
                    _ProgramDescriptions.Add(tmp);
                }
                return _ProgramDescriptions;
            }
            set
            {
                if (!_ProgramDescriptions.Equals(value))
                {
                    this._ProgramDescriptions = value;
                    var offset = 96 + (this.HasPointer ? 8 : 0);
                    var lenCounter = 0;
                    foreach (var i in value)
                    {
                        var tmp = (Descriptor)i;
                        lenCounter += (tmp.Length + 2);
                        var buffer = tmp.GetBytes();
                        this.Data.WriteBlock(buffer, offset + (lenCounter * 8), buffer.Length * 8);
                    }
                    if (this.ProgramInfoLength != lenCounter)
                    {
                        this.ProgramInfoLength = lenCounter;
                    }
                }
            }
        }


        public struct Track
        {
            /// <summary>
            /// This 8-bits field specifying the tpye of program element 
            /// carried within the packets with the PID whose value is specified by 
            /// <see cref="ElementaryPID"/>.
            /// </summary>
            public byte StreamType { get; set; }
            /// <summary>
            /// This 13-bie field specifying the PID of the Transport
            /// Stream packets which carry the associated program element.
            /// <para>See ISO/IEC13818-1 Section2.4.4.8 Table2-29 for Stream type assignments.</para>
            /// </summary>
            public int ElementaryPID { get; set; }
            /// <summary>
            /// This 12-bit field is the length of <see cref="ESInfoDescriptions"/> in byte. 
            /// first two bits allways be '00'.
            /// </summary>
            public int ES_Info_Length { get; set; }

            public List<Descriptor> ESInfoDescriptions { get; set; }

            public Track(BitPacket packet)
            {
                this.StreamType = packet.ReadByte();
                packet.SkipBit(3);
                this.ElementaryPID = packet.ReadInt(13);
                packet.SkipBit(4);
                this.ES_Info_Length = packet.ReadInt(12);
                if (this.ES_Info_Length > 0)
                {
                    ESInfoDescriptions = new List<Descriptor>();
                    int counter = 0;
                    while (counter < this.ES_Info_Length)
                    {
                        int pos = packet.Position;
                        ESInfoDescriptions.Add(new Descriptor(packet));
                        counter += (packet.Position - pos) / 8;
                    }
                }
                else
                {
                    ESInfoDescriptions = null;
                }
            }

            public byte[] GetBytes()
            {
                var result = new BitPacket();
                result.WriteByte(this.StreamType, 8);
                result.WriteInt(0, 3);
                result.WriteInt(this.ElementaryPID, 13);
                result.WriteInt(0, 4);
                result.WriteInt(this.ES_Info_Length, 12);
                foreach (var i in this.ESInfoDescriptions)
                {
                    var tmp = i.GetBytes();
                    result.WriteBlock(tmp, tmp.Length * 8);
                }
                return result.ToByteArray();
            }

        }


        private List<Track> _TrackList;
        public List<Track> TrackList
        {
            get
            {
                //5byte...+ CRC 4byte total len = seclen-9
                if (_TrackList != null) return _TrackList;
                var offset = 96 + (this.HasPointer ? 8 : 0) + (this.ProgramInfoLength * 8);
                var counter = 13 + this.ProgramInfoLength;
                _TrackList = new List<Track>();
                var packet = new BitPacket(this.Data.ReadBlock(offset, (SectionLength - counter) * 8));
                while (counter < this.SectionLength)
                {
                    var tmp = new Track(packet);
                    counter += 5 + tmp.ES_Info_Length;
                    _TrackList.Add(tmp);
                }
                return _TrackList;
            }
            set
            {
                if (!_TrackList.Equals(value))
                {
                    this._TrackList = value;
                    var offset = 96 + (this.HasPointer ? 8 : 0) + (this.ProgramInfoLength * 8);
                    var lenCounter = 0;
                    foreach (var i in value)
                    {
                        lenCounter += (5 + i.ES_Info_Length) * 8;
                        var buffer = i.GetBytes();
                        this.Data.WriteBlock(buffer, offset + (lenCounter * 8), buffer.Length * 8);
                    }
                    lenCounter += 13 + this.ProgramInfoLength;
                    if (this.SectionLength != lenCounter)
                    {
                        this.SectionLength = lenCounter;
                    }
                }
            }
        }

        public uint CRC32
        {
            get => this.Data.ReadUInt(24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
            set => this.Data.WriteInt(value, 24 + (this.HasPointer ? 8 : 0) + (this.SectionLength * 8 - 32), 32);
        }
        public BitPacket Data { get; set; }

        public PMTPacket(BitPacket packet, bool hasPointer)
        {
            this.Data = packet;
            this.HasPointer = hasPointer;
            #region Old Method
            //if (hasPointer)
            //{
            //    this.Pointer_Field = packet.ReadByte();
            //}
            //this.TableID = packet.ReadByte();
            //this.SyntaxIndicator = packet.ReadBool();
            //this.IsPrivate = packet.ReadBool();
            //this.PSIReserved = packet.ReadByte(2);
            //packet.SkipBit(2); // The sectionLengths first 2 bits should allways be '00'
            //this.SectionLength = packet.ReadInt(10);

            //this.ProgramNumber = packet.ReadInt(16);
            //packet.SkipBit(2);
            //this.Version = packet.ReadByte(5);
            //this.IsApplicable = packet.ReadBool();
            //this.SectionNumber = packet.ReadByte();
            //this.LastSectionNumber = packet.ReadByte();
            //packet.SkipBit(3);
            //this.PCR_PID = packet.ReadInt(13);
            //packet.SkipBit(4);
            //packet.SkipBit(2);
            //this.ProgramInfoLength = packet.ReadInt(10);
            //int counter = 0;
            //if (this.ProgramInfoLength > 0)
            //{
            //    this.ProgramDescriptions = new List<Descriptor>();
            //    while (counter < this.ProgramInfoLength)
            //    {
            //        int pos = packet.Position;
            //        ProgramDescriptions.Add(new Descriptor(packet));
            //        counter += (packet.Position - pos) / 8;
            //    }
            //}
            //counter += 4; // put CRC32 length into it.
            //if(counter < this.SectionLength)
            //{
            //    this.ProgramList = new List<Program>();
            //    while (counter < this.SectionLength)
            //    {
            //        var pos = packet.Position;
            //        this.ProgramList.Add(new Program(packet));
            //        counter += (packet.Position - pos) / 8; // might couse problem.
            //    }
            //}
            //this.CRC32 = (uint)packet.ReadInt(32);
            #endregion
        }

        public PMTPacket(byte[] packet, bool hasPointer)
        {
            this.Data = new BitPacket(packet);
            this.HasPointer = hasPointer;
        }
    }
}
