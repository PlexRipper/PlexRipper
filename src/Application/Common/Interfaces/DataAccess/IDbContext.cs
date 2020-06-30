using Microsoft.EntityFrameworkCore;
using System;

namespace PlexRipper.Application.Common.Interfaces.DataAccess
{
    public interface IDbContext : IDisposable
    {
        public DbContext Instance { get; }
    }
}
