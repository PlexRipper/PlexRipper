using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace PlexRipper.Identity.Contracts;

public interface IAuthDbContext : IDataProtectionKeyContext, IDisposable { }
