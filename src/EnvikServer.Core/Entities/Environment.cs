namespace EnvikServer.Core.Entities;

public class Environment
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[] EncryptedData { get; set; }
    public string? EncryptionMetadata { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public User LastModifier { get; set; } = null!;
}