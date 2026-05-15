using System.ComponentModel.DataAnnotations;

namespace Loop.Web.Api.Controllers.Requests;

public class AddPointsToUserRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Points amount must be greater than 0")]
    public int Amount { get; set; }

}
