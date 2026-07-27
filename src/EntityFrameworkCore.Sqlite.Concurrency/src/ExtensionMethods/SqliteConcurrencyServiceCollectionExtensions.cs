using EntityFrameworkCore.Sqlite.Concurrency.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityFrameworkCore.Sqlite.Concurrency.ExtensionMethods;

/// <summary>
/// Extension methods for IServiceCollection to add concurrent SQLite DbContexts.
/// </summary>
public static class SqliteConcurrencyServiceCollectionExtensions
{
    /// <summary>
    /// Adds a DbContext configured with optimized SQLite concurrency and performance settings.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <param name="configure">An optional action to configure concurrency options.</param>
    /// <param name="contextLifetime">The lifetime of the DbContext.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// This overload automatically resolves <see cref="ILoggerFactory"/> from the DI
    /// container and injects it into the concurrency options so that <c>SQLITE_BUSY*</c>
    /// events and <c>BEGIN IMMEDIATE</c> upgrades are logged through the application's
    /// normal logging pipeline.
    /// </para>
    /// <para>
    /// A <see cref="DbContext"/> instance is <b>not thread-safe</b>. For workloads that
    /// create concurrent database operations (e.g. <c>Task.WhenAll</c>, background queues,
    /// hosted services), use
    /// <see cref="AddConcurrentSqliteDbContextFactory{TContext}(IServiceCollection, string, Action{SqliteConcurrencyOptions}?, ServiceLifetime)"/>
    /// instead and inject <see cref="Microsoft.EntityFrameworkCore.IDbContextFactory{TContext}"/>
    /// to create a separate context per concurrent operation.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddConcurrentSqliteDbContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<SqliteConcurrencyOptions>? configure = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((provider, options) =>
        {
            options.UseSqliteWithConcurrency(connectionString, o =>
            {
                configure?.Invoke(o);

                // Inject the singleton ILoggerFactory so the interceptor can emit
                // structured logs without the caller having to wire it up manually.
                if (o.LoggerFactory is null)
                    o.LoggerFactory = provider.GetService<ILoggerFactory>();
            });
        }, contextLifetime);

        return services;
    }

    /// <summary>
    /// Adds an <see cref="Microsoft.EntityFrameworkCore.IDbContextFactory{TContext}"/>
    /// configured with optimized SQLite concurrency and performance settings.
    /// </summary>
    /// <typeparam name="TContext">The type of the DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <param name="configure">An optional action to configure concurrency options.</param>
    /// <param name="factoryLifetime">
    /// The lifetime of the factory. Defaults to <see cref="ServiceLifetime.Singleton"/>
    /// because the factory itself holds no per-request state and is safe to share.
    /// </param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// <para>
    /// Prefer this overload whenever operations execute concurrently — for example inside
    /// <c>Task.WhenAll</c>, <c>Channel&lt;T&gt;</c> consumers,
    /// <c>IHostedService</c> workers, or
    /// <c>Parallel.ForEachAsync</c>. Each concurrent flow should call
    /// <see cref="Microsoft.EntityFrameworkCore.IDbContextFactory{TContext}.CreateDbContext"/>
    /// to obtain its own independent context instance, which eliminates EF Core's
    /// object-level thread-safety restriction entirely.
    /// </para>
    /// <para>
    /// This overload automatically resolves <see cref="ILoggerFactory"/> from the DI
    /// container so that <c>SQLITE_BUSY*</c> events and <c>BEGIN IMMEDIATE</c> upgrades
    /// are logged through the application's normal logging pipeline.
    /// </para>
    /// <example>
    /// <code>
    /// // Registration
    /// builder.Services.AddConcurrentSqliteDbContextFactory&lt;AppDbContext&gt;(
    ///     "Data Source=app.db");
    ///
    /// // Concurrent use — each task gets its own context
    /// public async Task ProcessAllAsync(IEnumerable&lt;int&gt; ids, CancellationToken ct)
    /// {
    ///     var tasks = ids.Select(async id =>
    ///     {
    ///         await using var db = _factory.CreateDbContext();
    ///         // ... read and write with db
    ///     });
    ///     await Task.WhenAll(tasks);
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public static IServiceCollection AddConcurrentSqliteDbContextFactory<TContext>(
        this IServiceCollection services,
        string connectionString,
        Action<SqliteConcurrencyOptions>? configure = null,
        ServiceLifetime factoryLifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>((provider, options) =>
        {
            options.UseSqliteWithConcurrency(connectionString, o =>
            {
                configure?.Invoke(o);

                // Inject the singleton ILoggerFactory so the interceptor can emit
                // structured logs without the caller having to wire it up manually.
                if (o.LoggerFactory is null)
                    o.LoggerFactory = provider.GetService<ILoggerFactory>();
            });
        }, factoryLifetime);

        return services;
    }
}
