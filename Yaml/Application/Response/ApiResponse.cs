using System.Net;

namespace Yaml.Application.Response;

public class ApiResponse<T>
{
    public HttpStatusCode Status { get; set; }
    public string? Title { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
}