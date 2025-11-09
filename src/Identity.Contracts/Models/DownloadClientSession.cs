using System.ComponentModel.DataAnnotations;

namespace Reaparr.Identity.Contracts;

public class DownloadClientSession
{
    [Key]
    public required string Sid { get; set; }
    
    public required DateTimeOffset ExpiresAt { get; set; }
    
    public required string Username { get; set; }
}


