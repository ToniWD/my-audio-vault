using Microsoft.AspNetCore.Identity;
using MusicStream.Core.Enums;

namespace MusicStream.Core.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Unknown;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User() : base() { }
}