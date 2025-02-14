using QRCoder;

namespace Bridge.Core;

public class RoomConfigurations
{
    public uint? RoomCreationExpirationMinutes { get; set; }
    
    public uint? RoomResurrectionExpirationMinutes { get; set; }
}