using System.Data.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Data.Contracts;

public interface IPlexRipperDbContextDatabase : IDbContextDatabaseHelpers { }
