namespace EnvikServer.Core.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<Environment> Environments  { get; set; } = new List<Environment>();
}