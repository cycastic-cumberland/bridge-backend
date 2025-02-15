using Bridge.Core;
using Bridge.Core.Dtos;
using Bridge.Domain;
using Bridge.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly ItemService _itemService;

    public ItemsController(ItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Page<ItemDto>), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<Page<ItemDto>> Query(Guid roomId,
        int pageNumber = 1,
        int itemPerPage = 5,
        CancellationToken cancellationToken = default)
    {
        return _itemService.GetLatestItemsAsync(roomId,
            new()
            {
                PageNumber = pageNumber,
                ItemPerPage = itemPerPage
            },
            cancellationToken);
    }

    [HttpGet("upload-presigned")]
    [ProducesResponseType(typeof(UploadPreSignedDto), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<UploadPreSignedDto> GetPreSignedUploadUrl(Guid roomId,
        string fileName,
        CancellationToken cancellationToken)
    {
        return _itemService.GetPreSignedUploadUrlAsync(roomId, fileName, cancellationToken);
    }

    [HttpPost("ready")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task MakeReady(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        return _itemService.MakeReadyAsync(roomId, itemId, cancellationToken);
    }

    [HttpGet("download-presigned")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<string> GetPreSignedDownloadUrl(Guid roomId,
        long itemId,
        CancellationToken cancellationToken)
    {
        return _itemService.GetPreSignedDownloadUrlAsync(roomId, itemId, cancellationToken);
    }
}