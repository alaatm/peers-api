using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Peers.Modules.SystemInfo.Queries;

public static class GetSystemInfo
{
    [Authorize(Roles = Roles.Staff)]
    public sealed record Query() : IQuery;

    public sealed record Response(
        Response.AppInfo App,
        Response.ServerInfo Server,
        Response.DatabaseInfo Database)
    {
        public sealed record AppInfo(
            string Version,
            DateTime BuildDate);

        public sealed record ServerInfo(
            string OSVersion,
            string RuntimeVersion);

        public sealed record DatabaseInfo(
            string? Version,
            string? DbSize,
            string? Unallocated,
            string? Reserved,
            string? Data,
            string? IndexSize,
            string? Unused);
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        private const string VerQuery = "select @@version";
        private const string SizeQuery = "sp_spaceused";

        private readonly IConfiguration _config;

        public Handler(IConfiguration config) => _config = config;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var buildDate = File.GetLastWriteTimeUtc(assembly.Location);
            var versionAttr = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0] as AssemblyInformationalVersionAttribute;

            var appInfo = new Response.AppInfo(versionAttr!.InformationalVersion, buildDate);
            var serverInfo = new Response.ServerInfo(Environment.OSVersion.ToString(), Environment.Version.ToString());
            var dbInfo = await GetDbInfo(ctk);

            return Result.Ok(new Response(appInfo, serverInfo, dbInfo));

            async Task<Response.DatabaseInfo> GetDbInfo(CancellationToken ctk)
            {
                using var conn = new SqlConnection(_config.GetConnectionString("Default"));
                using var sqlCmd = conn.CreateCommand();

#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    await conn.OpenAsync(ctk);
                    string? dbServerVersion = null;
                    string? dbSize = null;
                    string? unallocated = null;
                    string? reserved = null;
                    string? data = null;
                    string? indexSize = null;
                    string? unused = null;

                    sqlCmd.CommandText = VerQuery;
                    dbServerVersion = await sqlCmd.ExecuteScalarAsync(ctk) as string;

                    sqlCmd.CommandText = SizeQuery;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    using var reader = await sqlCmd.ExecuteReaderAsync(ctk);

                    if (await reader.ReadAsync(ctk))
                    {
                        dbSize = reader.GetString(1);
                        unallocated = reader.GetString(2);
                    }

                    if (await reader.NextResultAsync(ctk) && await reader.ReadAsync(ctk))
                    {
                        reserved = reader.GetString(0);
                        data = reader.GetString(1);
                        indexSize = reader.GetString(2);
                        unused = reader.GetString(3);
                    }

                    return new Response.DatabaseInfo(
                        dbServerVersion,
                        dbSize,
                        unallocated,
                        reserved,
                        data,
                        indexSize,
                        unused);
                }
                catch
                {
                    return new Response.DatabaseInfo(null, null, null, null, null, null, null);
                }
                finally
                {
                    await conn.CloseAsync();
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
    }
}
