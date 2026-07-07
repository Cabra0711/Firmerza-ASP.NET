namespace Firmeza.DTOs;

public class ImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
