using System.Data;
using System.Data.Common;

namespace Publo.Postgres.Infrastructure.Npgsql;

internal static class DbDataReaderExtensions
{
    public static string? GetNullableString(this DbDataReader reader, string name)
        => reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetString(name);
}
