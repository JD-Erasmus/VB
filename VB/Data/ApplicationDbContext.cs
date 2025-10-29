using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VB.Models;

namespace VB.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vault> Vault { get; set; }
        public DbSet<VaultShare> VaultShares { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Vault>(entity =>
            {
                entity.HasIndex(v => v.UserId);

                entity.HasOne<IdentityUser>()
                    .WithMany()
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<VaultShare>(entity =>
            {
                entity.HasIndex(vs => vs.TokenHash).IsUnique();
                entity.HasIndex(vs => new { vs.VaultId, vs.OwnerUserId });

                entity.Property(vs => vs.TokenHash)
                    .HasMaxLength(128)
                    .IsRequired();

                entity.Property(vs => vs.RecipientNote)
                    .HasMaxLength(200);

                entity.Property(vs => vs.EncryptedPayload)
                    .IsRequired();

                entity.HasOne(vs => vs.Vault)
                    .WithMany()
                    .HasForeignKey(vs => vs.VaultId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
