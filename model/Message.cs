using System;
using System.Collections.Generic;

namespace backend.model;

public partial class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string? Content { get; set; }

    public string? MediaUrl { get; set; }

    public int? ReplyToMessageId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Message> InverseReplyToMessage { get; set; } = new List<Message>();

    public virtual Message? ReplyToMessage { get; set; }
}
