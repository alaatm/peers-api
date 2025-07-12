using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Mashkoor.Modules.System.Queries;

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
            string? ServiceLevelObjective,
            int? DtuLimit,
            int? CpuLimit,
            int? MinCores,
            int? MaxDop,
            int? MaxSessions,
            int? MaxDbMemory,
            long? MaxDbMaxSizeInMb,
            int? CheckpointRateIO,
            int? CheckpointRateMbps,
            int? PrimaryGroupMaxWorkers,
            long? PrimaryMaxLogRate,
            int? PrimaryGroupMaxIO,
            double? PrimaryGroupMaxCpu,
            int? VolumeTypeManagedXstoreIOPS);
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        private const string VerQuery = "select @@version";
        private const string SpecsQuery = "select slo_name,dtu_limit,cpu_limit,min_cores,max_dop,max_sessions,max_db_memory,max_db_max_size_in_mb,checkpoint_rate_io,checkpoint_rate_mbps,primary_group_max_workers,primary_max_log_rate,primary_group_max_io,primary_group_max_cpu,volume_type_managed_xstore_iops from sys.dm_user_db_resource_governance";

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
                    sqlCmd.CommandText = VerQuery;
                    var dbServerVersion = (await sqlCmd.ExecuteScalarAsync(ctk) as string)!;

                    try
                    {
                        sqlCmd.CommandText = SpecsQuery;
                        using var reader = await sqlCmd.ExecuteReaderAsync(ctk);
                        await reader.ReadAsync(ctk);

                        return new Response.DatabaseInfo(
                            dbServerVersion,
                            reader.GetString(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetInt16(3),
                            reader.GetInt16(4),
                            reader.GetInt32(5),
                            reader.GetInt32(6),
                            reader.GetInt64(7),
                            reader.GetInt32(8),
                            reader.GetInt32(9),
                            reader.GetInt32(10),
                            reader.GetInt64(11),
                            reader.GetInt32(12),
                            reader.GetDouble(13),
                            reader.GetInt32(14));
                    }
                    catch
                    {
                        // This throws on non-Azure SQL DBs
                        return new Response.DatabaseInfo(dbServerVersion, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
                    }
                }
                catch
                {
                    return new Response.DatabaseInfo(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
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
