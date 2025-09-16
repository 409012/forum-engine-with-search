using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEwS.Forums.Storage.Entities;

public class Comment
{
    [Key]
    public Guid CommentId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public Guid TopicId { get; init; }

    public Guid UserId { get; init; }

    [ForeignKey(nameof(UserId))]
    public User? Author { get; init; }

    [ForeignKey(nameof(TopicId))]
    public Topic? Topic { get; init; }

    [MaxLength(5000)]
    public required string Text { get; init; }
}