namespace TSRawStreamMarker.TransportStream
{
    /// <summary>
    /// Adaptation field control
    /// </summary>
    public enum  AdaptationField
    {
        /// <summary>
        /// no adaptation field, payload only
        /// </summary>
        None = 0b01,
        /// <summary>
        /// adaptation field only, no payload
        /// </summary>
        AdaptationOnly = 0b10,
        /// <summary>
        /// adaptation field followed by payload
        /// </summary>
        AdaptationWithPayload = 0b11,
        /// <summary>
        /// RESERVED for future use
        /// </summary>
        RESERVED = 0b00
    }
}
