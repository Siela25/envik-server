using EnvikServer.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Environment = EnvikServer.Core.Entities.Environment; // Todo: make it prettier

namespace EnvikServer.Infrastructure.Data;

public class EnvikDbContext : DbContext
{
    public EnvikDbContext(DbContextOptions<EnvikDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<OAuthAccount> OAuthAccounts { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<OrganizationMember> OrganizationMembers { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Environment> Environments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(255).IsRequired();
            entity.Property(user => user.Name).HasMaxLength(255).IsRequired();

            entity.HasMany(user => user.OAuthAccounts)
                .WithOne(oAuthAccount => oAuthAccount.User)
                .HasForeignKey(oAuthAccount => oAuthAccount.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // OAuthAccount
        modelBuilder.Entity<OAuthAccount>(entity =>
        {
            entity.HasKey(account => account.Id);
            entity.HasIndex(account => new { account.Provider, account.ProviderUserId }).IsUnique();
            entity.Property(account => account.Provider).HasMaxLength(50).IsRequired();
            entity.Property(account => account.ProviderUserId).HasMaxLength(255).IsRequired();
        });
        
        // Organization
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(organization => organization.Id);
            entity.HasIndex(organization => organization.Slug).IsUnique();
            entity.Property(organization => organization.Slug).HasMaxLength(255).IsRequired();
            entity.Property(organization => organization.Name).HasMaxLength(255).IsRequired();

            entity.HasOne(organization => organization.Owner)
                .WithMany(user => user.OwnedOrganizations)
                .HasForeignKey(organization => organization.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(organization => organization.Members)
                .WithOne(user => user.Organization)
                .HasForeignKey(user => user.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

        });
        
        // OrganizationMember
        modelBuilder.Entity<OrganizationMember>(entity =>
        {
            entity.HasKey(organizationMember => organizationMember.Id);
            entity.HasIndex(organizationMember => new { organizationMember.OrganizationId, organizationMember.UserId });
        });
        
        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(project => project.Id);
            entity.HasIndex(project => new { project.OrganizationId, project.Slug }).IsUnique();

            entity.HasOne(project => project.Organization)
                .WithMany(organization => organization.Projects)
                .HasForeignKey(project => project.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(project => project.Creator)
                .WithMany()
                .HasForeignKey(project => project.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Environment
        modelBuilder.Entity<Environment>(entity =>
        {
            entity.HasKey(env => env.Id);
            entity.HasIndex(env => new { env.ProjectId, env.Name }).IsUnique();

            entity.HasOne(env => env.Project)
                .WithMany(project => project.Environments)
                .HasForeignKey(env => env.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(env => env.LastModifier)
                .WithMany()
                .HasForeignKey(env => env.LastModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}