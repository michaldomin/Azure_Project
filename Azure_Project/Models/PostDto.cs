namespace Azure_Project.Models;
public class PostDto
{
    public int Id { get; set; }
    public UserDto User { get; set; }
    public DateTime CreationDate { get; set; }
    public string Text { get; set; }
    public double? Polarity { get; set; }
    public double? Subjectivity { get; set; }
}
