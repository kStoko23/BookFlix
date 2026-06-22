using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class RefreshTokenConfiguration: IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .IsRequired();
        builder.Property(x => x.TokenHash)
            .HasColumnName("TokenHash")
            .IsRequired();
        builder.Property(x => x.UserId)
            .HasColumnName("UserId")
            .IsRequired();
        builder.Property(x => x.ExpiresAt)
            .HasColumnName("ExpiresAt")
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        builder.Property(x => x.Revoked)
            .HasColumnName("Revoked")
            .HasDefaultValue(false);
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}