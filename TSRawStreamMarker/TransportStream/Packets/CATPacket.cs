using System.Collections.Generic;
using TSRawStreamMarker.TransportStream.Packets.Descriptors;

namespace TSRawStreamMarker.TransportStream.Packets
{
    /// <summary>
    /// Conditional access(CA) Table
    /// <para>This table provides the association between one or more CA systems,
    /// their EMM streams and any special parameters associated with them.</para>
    /// </summary>
    public class CATPacket : IPSISection
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

        private List<CADescriptor> _CADescriptors;
        public List<CADescriptor> CADescriptors
        {
            get
            {
                //5byte...+ CRC 4byte total len = seclen-9
                if (_CADescriptors != null) return _CADescriptors;
                var offset = 64 + (this.HasPointer ? 8 : 0);
                var counter = 9;
                _CADescriptors = new List<CADescriptor>();
                var packet = new BitPacket(this.Data.ReadBlock(offset, (SectionLength - 9) * 8));
                while(counter < this.SectionLength)
                {
                    var tmp = new CADescriptor(packet);
                    counter += (tmp.Length + 2);
                    _CADescriptors.Add(tmp);
                }
                return _CADescriptors;
            }
            set
            {
                if (!_CADescriptors.Equals(value))
                {
                    this._CADescriptors = value;
                    var offset = 64 + (this.HasPointer ? 8 : 0);
                    var lenCounter = 0;
                    foreach(var i in value)
                    {
                        var tmp = (Descriptor)(IDescriptor)i;
                        lenCounter += (tmp.Length + 2);
                        var buffer = tmp.GetBytes();
                        this.Data.WriteBlock(buffer, offset + (lenCounter * 8), buffer.Length * 8);
                    }
                    lenCounter += 9;
                    if(this.SectionLength != lenCounter)
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

        public CATPacket(BitPacket packet, bool hasPointer)
        {
            this.HasPointer = hasPointer;
            this.Data = packet;
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
            ////this.Reserved = packet.ReadBlock(18);
            //this.Version = packet.ReadByte(5);
            //this.IsApplicable = packet.ReadBool();
            //this.SectionNumber = packet.ReadByte();
            //this.LastSectionNumber = packet.ReadByte();
            //var counter = 5 + 4;
            //this.CADescriptors = new List<CADescriptor>();
            //while (counter < this.SectionLength)
            //{
            //    var pos = packet.Position;
            //    this.CADescriptors.Add(new CADescriptor(packet));
            //    counter += (packet.Position - pos)/8; // might couse problem.
            //}
            //this.CRC32 = (uint)packet.ReadInt(32);
        }
    }
}
