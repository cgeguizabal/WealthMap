using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;

namespace WealthMap.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly IEncryptionService _encryption;

    public NotificationConfiguration(IEncryptionService encryption) => _encryption = encryption;

    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(n => n.Severity)
            .IsRequired()
            .HasConversion<int>();

        // Encrypted, though the task's column list did not name it. The title is a
        // built sentence: "'Visa Clasica' payment due in 2 day(s)" — the card's
        // nickname is inside it. Encrypting params while leaving this in plaintext
        // would protect nothing.
        builder.Property(n => n.Title)
            .IsRequired()
            .IsEncrypted(_encryption);

        // Same, and worse: the message carries the name and the amount together.
        builder.Property(n => n.Message)
            .IsRequired()
            .IsEncrypted(_encryption);

        // Serialised, then encrypted. The audit this task asked for found that
        // params carries ["name"] on three alert rules — the card, debt or goal
        // nickname the sentence was built from.
        //
        // The column type drops from jsonb to text as a result. Nothing queried
        // inside it, so no index or operator is lost; the JSON structure survives
        // intact because it is serialised before the encryption sees it.
        builder.Property(n => n.Params)
            .HasColumnName("params")
            .HasColumnType("text")
            .IsRequired()
            .HasConversion(
                v => _encryption.Encrypt(JsonSerializer.Serialize(v, JsonOptions)),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(
                         _encryption.Decrypt(v), JsonOptions)
                     ?? new Dictionary<string, string>())
            // Without a comparer EF compares dictionaries by reference and reports
            // a change on every load, rewriting rows that never changed.
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyDictionary<string, string>>(
                (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(
                         JsonSerializer.Serialize(v, JsonOptions), JsonOptions)
                     ?? new Dictionary<string, string>()));

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}
