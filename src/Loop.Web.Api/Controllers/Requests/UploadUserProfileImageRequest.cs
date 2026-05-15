using Microsoft.AspNetCore.Http;

namespace Loop.Web.Api.Controllers.Requests;

public class UploadUserProfileImageRequest
{
    public IFormFile ImageFile { get; set; } = null!;
}
