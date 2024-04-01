public class UserLoginResponse
{
    public string? Token { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess;
}