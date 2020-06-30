using Microsoft.EntityFrameworkCore;
using System;

namespace PlexRipper.Data.Common.Interfaces
{
    public interface IDbContext : IDisposable
    {
        public DbContext Instance { get; }
    }
}
