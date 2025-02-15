using Bridge.Core;
using Bridge.Infrastructure.Data;

namespace Bridge.Web;

public class AppSettings
{
    public required string FrontendUrl { get; set; }

    public string AllowedOrigins { get; set; } = "";
    
    public required ItemConfigurations ItemConfigurations { get; set; }
    
    public required RoomConfigurations RoomConfigurations { get; set; }

    public required PasteConfigurations PasteConfigurations { get; set; }
    
    public required S3Settings S3Settings { get; set; }
}