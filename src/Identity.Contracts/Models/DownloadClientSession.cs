using System.ComponentModel.DataAnnotations;

namespace Reaparr.Identity.Contracts;

public class DownloadClientSession
{
    [Key]
    [MaxLength(255)]
    public required string Sid { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    [MaxLength(255)]
    public required string Username { get; set; }
}
