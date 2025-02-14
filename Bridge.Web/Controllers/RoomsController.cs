using Bridge.Core;
using Bridge.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;
    private readonly QrService _qrService;
    private readonly IUrlGenerator _urlGenerator;

    public RoomsController(RoomService roomService, QrService qrService, IUrlGenerator urlGenerator)
    {
        _roomService = roomService;
        _qrService = qrService;
        _urlGenerator = urlGenerator;
    }

    [HttpGet("new-room")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<string> CreateNewRoom(CancellationToken cancellationToken)
    {
        return (await _roomService.CreateRoomAsync(cancellationToken)).ToString();
    }
    
    [HttpGet("qr/{roomId:guid}")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ResponseCache(Duration = int.MaxValue)]
    public FileContentResult GenerateQr(Guid roomId)
    {
        var png = _qrService.GeneratePngQrCode(_urlGenerator.GetFrontendRoomUrl(roomId));
        return File(png, "image/png");
    }

    [HttpGet("exists/{roomId:guid}")]
    [ProducesResponseType(typeof(bool), 200)]
    public Task<bool> RoomExists(Guid roomId, CancellationToken cancellationToken)
    {
        return _roomService.ExistsAsync(roomId, cancellationToken);
    }
}