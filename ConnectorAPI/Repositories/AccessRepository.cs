using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace ConnectorAPI.Repositories;

public class AccessRepository
{
    private readonly ConnectorDbContext _db;
    private readonly IMemoryCache _memory;
    private readonly ILogger<AccessRepository> _logger;

    private static readonly Func<ConnectorDbContext, string, string, string, string, short, Connection?> GetConnectionQuery =
    EF.CompileQuery(
        (ConnectorDbContext db, string ownerId, string guestId, string resourceId, string resourceName, short accessLevel) => db
                .Connections
                .Where(cn => cn.OwnerId == ownerId && cn.GuestId == guestId)
                .Include(c => c.Resources.Where(r => r.ResourceId == resourceId && r.ResourceName == resourceName))
                    .ThenInclude(r => r.Attributes.Where(at => accessLevel >= at.MinimumAccessLevel))
                .AsSingleQuery()
                .AsNoTracking()
                .SingleOrDefault()
    );

    public AccessRepository(ConnectorDbContext db, IMemoryCache memory, ILogger<AccessRepository> logger)
    {
        _db = db;
        _memory = memory;
        _logger = logger;
    }

    public Connection? GetConnection(string guestId, AccessRequest request)
    {
        return GetConnection(request.OwnerId, guestId, request.ResourceId, request.ResourceName, request.AccessLevel);
    }

    public Connection? GetConnection(string guestId, UpdateRequest request)
    {
        return GetConnection(request.OwnerId, guestId, request.ResourceId, request.ResourceName, request.AccessLevel);
    }

    public Connection? GetConnection(string ownerId, string guestId, string resourceId, string resourceName, short accessLevel)
    {
        string key = string.Join("::", ownerId, guestId, resourceId, resourceName, accessLevel);

        return _memory.GetOrCreate(
            key,
            (entry) =>
            {
                _logger.LogInformation("Cache miss for key {0}", key);

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return GetConnectionQuery(_db, ownerId, guestId, resourceId, resourceName, accessLevel);
            }
        );
    }
}