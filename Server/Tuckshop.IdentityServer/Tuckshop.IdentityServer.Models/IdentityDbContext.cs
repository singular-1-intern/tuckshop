namespace Tuckshop.IdentityServer
{
  using System;
  using System.Collections.Generic;
  using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore;
  using Neo.IdentityServer.Models;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.IdentityServer.Models.SignIn;
  using Neo.Model.Processing;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// Provides Identity Db Context.
  /// </summary>
  public class IdentityDbContext : IdentityDbContextBase<IdentityDbContext, Models.TuckshopApplicationUser>, IIdentityProvidersDbContext, ISignInAuditDbContext, IDataProtectionKeyContext
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContextOptions.</param>
    /// <param name="dbContextProcessors">Db Context processors.</param>
    public IdentityDbContext(
      DbContextOptions<IdentityDbContext> options,
      IEnumerable<IDbContextProcessor> dbContextProcessors)
        : base(options, dbContextProcessors)
    {
    }

    /// <inheritdoc />
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    /// <inheritdoc/>
    public DbSet<IdentityProvider> IdentityProviders { get; set; }

    /// <inheritdoc/>
    public DbSet<SignInAudit> SignInAudits { get; set; }

    /// <summary>
    /// Gets or sets the UserManagementActionLog DbSet.
    /// </summary>
    public DbSet<UserManagementActionLog> UserManagementActionLogs { get; set; }

    public DbSet<UserInvite> UserInvites { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      _ = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));

      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Models.TuckshopApplicationUser>(entityTypeBuilder =>
      {
        entityTypeBuilder.Property(appUser => appUser.UserId)
          .ValueGeneratedOnAdd();

        entityTypeBuilder.HasOne(u => u.IdentityProvider).WithMany().IsRequired(false);

        entityTypeBuilder.HasAlternateKey(appUser => appUser.UserId).HasName("IX_UserId");
      });

      modelBuilder.Entity<IdentityProvider>(entityTypeBuilder =>
      {
        entityTypeBuilder.HasQueryFilter(identityProvider => identityProvider.Deleted!.On == null);
      });
    }
  }
}
