using Microsoft.EntityFrameworkCore;
using System;

namespace PlexRipper.Infrastructure.Common.Interfaces
{
    public interface IDbContext : IDisposable
    {
        DbContext Instance { get; }
    }
}
