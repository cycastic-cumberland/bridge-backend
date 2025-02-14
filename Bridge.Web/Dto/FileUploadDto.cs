using System.ComponentModel.DataAnnotations;

namespace Bridge.Web.Dto;

public class FileUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}