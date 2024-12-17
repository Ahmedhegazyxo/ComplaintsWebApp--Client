using System.ComponentModel.DataAnnotations.Schema;

namespace Base;

public class BaseEntity
{
    public int Id { get; set; } 
    public string? Notes { get; set; }
    public string? CreatorIPAddress { get; set; }
    public DateTime? CreationDate { get; set; }
    [NotMapped]
    public bool IsValidEntity { get; set; }
}
