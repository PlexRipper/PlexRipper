using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace Reaparr.Identity.Contracts;

public interface IAuthDbContext : IDataProtectionKeyContext, IDisposable { }
