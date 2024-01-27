using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Azure_Project.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\W).*$", ErrorMessage = "Password must contain at least one capital letter, one small letter, and one special character")]
    public string Password { get; set; }

}
