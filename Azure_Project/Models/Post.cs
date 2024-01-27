using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Azure_Project.Models;

public class Post
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string Text { get; set; }

    public double? Polarity { get; set; }

    public double? Subjectivity { get; set; }
}