namespace Loop.Web.Api.Controllers.Responses;

public class UploadUserProfileImageResponse
{
    public Guid UserId { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Message { get; set; } = "Image uploaded successfully";
}
