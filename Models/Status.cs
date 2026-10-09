using System.ComponentModel.DataAnnotations;

namespace abcomm_test.Models;

public class Status
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
