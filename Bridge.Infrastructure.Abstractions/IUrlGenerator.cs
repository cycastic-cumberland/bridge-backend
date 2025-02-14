namespace Bridge.Infrastructure;

public interface IUrlGenerator
{
    string GetFrontendRoomUrl(Guid roomId);
}