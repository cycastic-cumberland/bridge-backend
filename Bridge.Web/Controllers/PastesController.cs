using Bridge.Core;
using Bridge.Core.Dtos;
using Bridge.Domain;
using Bridge.Web.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PastesController : ControllerBase
{
    private readonly PasteService _pasteService;

    public PastesController(PasteService pasteService)
    {
        _pasteService = pasteService;
    }

    [HttpGet("pastes")]
    [ProducesResponseType(typeof(Page<PasteDto>), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<Page<PasteDto>> Query(Guid roomId,
        int pageNumber = 1,
        int itemPerPage = 5,
        CancellationToken cancellationToken = default)
    {
        return _pasteService.GetLatestPastesAsync(roomId,
            new()
            {
                PageNumber = pageNumber,
                ItemPerPage = itemPerPage,
            }, cancellationToken);
    }

    [HttpGet("paste")]
    [ProducesResponseType(typeof(PasteDto), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<PasteDto> GetPaste(Guid roomId, long pasteId, bool truncate, CancellationToken cancellationToken)
    {
        return _pasteService.GetPasteAsync(roomId, pasteId, truncate, cancellationToken);
    }

    [HttpPut]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionDto), 400)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task CreatePaste(Guid roomId, CreatePasteDto request, CancellationToken cancellationToken)
    {
        return _pasteService.CreatePasteAsync(roomId, request.Content, cancellationToken);
    }
}