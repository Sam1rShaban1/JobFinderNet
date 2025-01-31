using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobFinderNet.Data;

public class Job
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    
    [Required, StringLength(1000)]
    public string Description { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    public string EmployerId { get; set; }
    
    [ForeignKey("EmployerId")]
    public ApplicationUser Employer { get; set; }
}