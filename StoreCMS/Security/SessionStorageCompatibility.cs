using System;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;

namespace Treynessen.Security
{
    public static class SessionStorageCompatibility
    {
        public static void Validate(CMSDatabase db)
        {
            var connection = db.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                Func<string, long> count = sql => { command.CommandText = sql; return Convert.ToInt64(command.ExecuteScalar()); };
                string columns = " FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers'";
                bool valid = count("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name IN ('Users','ConnectedUsers') AND engine='InnoDB' AND table_type='BASE TABLE'") == 2
                    && count("SELECT COUNT(*)" + columns) == 6
                    && count("SELECT COUNT(*)" + columns + " AND column_name IN ('ID','UserID') AND data_type='int' AND is_nullable='NO' AND generation_expression=''") == 2
                    && count("SELECT COUNT(*)" + columns + " AND column_name='ID' AND column_key='PRI' AND extra='auto_increment'") == 1
                    && count("SELECT COUNT(*)" + columns + " AND column_name='TokenHash' AND data_type='char' AND character_maximum_length=64 AND character_set_name='ascii' AND collation_name='ascii_bin' AND is_nullable='NO' AND generation_expression=''") == 1
                    && count("SELECT COUNT(*)" + columns + " AND column_name='LastActionUtc' AND data_type='datetime' AND datetime_precision=6 AND is_nullable='NO' AND generation_expression=''") == 1
                    && count("SELECT COUNT(*)" + columns + " AND ((column_name='IPAdress' AND data_type='varchar' AND character_maximum_length=45) OR (column_name='UserAgent' AND data_type='varchar' AND character_maximum_length=255)) AND is_nullable='NO'") == 2
                    && count("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='Users' AND column_name='Password' AND data_type IN ('char','varchar','tinytext','text','mediumtext','longtext') AND character_maximum_length>=84 AND generation_expression=''") == 1;
                foreach (string column in new[] { "TokenHash", "UserID" })
                    valid = valid && count("SELECT COUNT(*) FROM (SELECT index_name FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND non_unique=0 GROUP BY index_name HAVING COUNT(*)=1 AND MIN(column_name)='" + column + "' AND SUM(sub_part IS NOT NULL)=0) AS session_indexes") == 1;
                valid = valid && count("SELECT COUNT(*) FROM information_schema.key_column_usage WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='UserID' AND referenced_table_name='Users' AND referenced_column_name='ID'") == 1;
                if (!valid) throw new InvalidOperationException("StoreCMS: требуется совместимая схема сессий V2 и InnoDB. Остановите сайт и проверьте SESSION_SECURITY_UPGRADE.md. Данные автоматически не изменяются.");
            }
        }
    }
}
