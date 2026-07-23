using System.Data;
using System.Data.Common;

namespace apexdojohub_API.Helpers
{
    public static class Helpers
    {
        public static DbCommand CreateStoredProcedure(this IDbConnection connection, string procedure)
        {
            var cmd = (DbCommand)connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procedure;

            return cmd;
        }

    }
}
