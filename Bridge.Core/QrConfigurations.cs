using QRCoder;

namespace Bridge.Core;

public class QrConfigurations
{
    public QRCodeGenerator.ECCLevel? EccLevel { get; set; }
    
    public uint? PixelPerModule { get; set; }
}