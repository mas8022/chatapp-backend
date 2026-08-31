using System;
using System.Collections.Generic;

namespace backend.model;

public partial class User
{
    public int Id { get; set; }

    public string Phone { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Username { get; set; } = null!;

    public string? Name { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
