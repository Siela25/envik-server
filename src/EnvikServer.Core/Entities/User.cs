namespace EnvikServer.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<OAuthAccount> OAuthAccounts { get; set; } = new List<OAuthAccount>();
    public ICollection<Organization> OwnedOrganizations { get; set; } = new List<Organization>();
    public ICollection<OrganizationMember> OrganizationMemeberships { get; set; } = new List<OrganizationMember>();
}

public class OAuthAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string? ProviderEmail { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}