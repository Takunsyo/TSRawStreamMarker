namespace TSRawStreamMarker.TransportStream
{
    public enum TextEncoding
    {
        ASCII = 0x01,
        Unicode = 0x001,
        UTF7 = 0x0001,
        UTF8 = 0x00001,
        UTF32 = 0x000001,
        LittleEndian = 0x0,
        BigEndian = 0x1,
    }
}
