using System.Collections;

namespace Bridge.Web.Dto;

public class ExceptionDto
{
    public required string Title { get; set; }
    public required int Status { get; set; }
    public required string Path { get; set; }
    public IDictionary? Data { get; set; }
}