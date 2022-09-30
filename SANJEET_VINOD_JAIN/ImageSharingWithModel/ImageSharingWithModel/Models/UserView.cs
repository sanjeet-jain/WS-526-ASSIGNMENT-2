using System.ComponentModel.DataAnnotations;

namespace ImageSharingWithModel.Models;

public class UserView
{
    public int Id { get; set; }

    [Required]
    [RegularExpression(@"[a-zA-Z0-9_]+", ErrorMessage = "UserName must be numbers and letters only without spaces or special characters!")]
    public string Username { get; set; }

    [Required] public bool ADA { get; set; }

    public bool IsADA()
    {
        // return "on".Equals(ADA);
        return ADA;
    }
}