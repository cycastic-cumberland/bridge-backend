using Bridge.Infrastructure;
using Microsoft.Extensions.Options;
using QRCoder;

namespace Bridge.Core;

public class QrService : ConfigurableService<QrConfigurations>
{
    public QrService(IAppDbContext dbContext,
        IOptions<QrConfigurations> configurations)
        : base(dbContext, configurations)
    {
    }

    public byte[] GeneratePngQrCode(string plainText)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(plainText, Configurations.EccLevel ?? QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic((int)(Configurations.PixelPerModule ?? 20U));
    }
}