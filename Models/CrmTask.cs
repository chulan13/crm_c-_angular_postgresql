using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace abcomm_test.Models;

public class CrmTask
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    [JsonPropertyName("status_id")]
    public Guid? StatusId { get; set; }
    [JsonPropertyName("department_id")]
    public Guid? DepartmentId { get; set; }
    public string? Assignee { get; set; }
    public DateOnly? Deadline { get; set; }
    public string? Description { get; set; }

    [StringLength(250, ErrorMessage = "причина закриття не може бути довшою за 250 символів")]
    [JsonPropertyName("cancel_reason")]
    public string? CancelReason { get; set; }
}
