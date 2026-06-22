using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class BookConfiguration: IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("Id")
                .IsRequired();
        builder.Property(x => x.Title)
                .HasColumnName("Title")
                .HasMaxLength(300)
                .IsRequired();
        builder.Property(x => x.Author)
                .HasColumnName("Author")
                .HasMaxLength(200)
                .IsRequired();
        builder.Property(x => x.Description)
                .HasColumnName("Description")
                .HasMaxLength(1200);
        builder.Property(x => x.Category)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
        builder.Property(x => x.Isbn)
                .HasColumnName("Isbn")
                .HasMaxLength(13)
                .IsRequired();
        builder.Property(x => x.Pages)
                .HasColumnName("Pages")
                .IsRequired();
        builder.Property(x => x.Rating)
                .HasColumnName("Rating")
                .IsRequired();
        builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();
        builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .IsRequired();
        
        builder.HasOne(x => x.User)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
