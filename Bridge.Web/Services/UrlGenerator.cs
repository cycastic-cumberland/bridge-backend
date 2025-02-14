using Bridge.Infrastructure;
using Microsoft.Extensions.Options;

namespace Bridge.Web.Services;

public class UrlGenerator : IUrlGenerator
{
    private readonly Uri _basePath;

    public UrlGenerator(IOptions<AppSettings> options)
    {
        _basePath = new(options.Value.FrontendUrl);
    }

    public string GetFrontendRoomUrl(Guid roomId)
    {
        return new Uri(_basePath, roomId.ToString()).ToString();
    }
}