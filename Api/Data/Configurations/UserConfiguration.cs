using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .IsRequired();
        builder.Property(x => x.Email)
            .HasColumnName("Email")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.PasswordHash)
            .HasColumnName("PasswordHash")
            .IsRequired();
        builder.Property(x => x.Username)
            .HasColumnName("Username")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Username).IsUnique();
    }
}