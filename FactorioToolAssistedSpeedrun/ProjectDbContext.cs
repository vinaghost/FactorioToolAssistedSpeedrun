using FactorioToolAssistedSpeedrun.Entities;
using FactorioToolAssistedSpeedrun.Enums;
using FactorioToolAssistedSpeedrun.Models.Database;
using FactorioToolAssistedSpeedrun.Services;

using Microsoft.Data.Sqlite;

using System.IO;

namespace FactorioToolAssistedSpeedrun
{
    public class ProjectDbContext : IDisposable
    {
        private static readonly string[] StepColumns =
        [
            "Id", "Name", "Location", "Type", "X", "Y", "Amount", "Item",
            "Orientation", "Inventory", "Priority", "Modifier", "Color", "Comment", "IsSkip"
        ];

        private static readonly string StepSelectColumns = string.Join(", ", StepColumns);

        private static readonly string InsertStepSql =
            $"INSERT INTO Steps ({StepSelectColumns}) VALUES ({string.Join(", ", StepColumns.Select(column => $"${column}"))})";

        private readonly string _name;
        private SqliteConnection? _connection;
        private SqliteTransaction? _transaction;

        public ProjectDbContext(string name) : this(name, waitForPendingWrites: true)
        {
        }

        internal ProjectDbContext(string name, bool waitForPendingWrites)
        {
            _name = name;
            if (waitForPendingWrites)
            {
                DatabaseWriteQueue.Flush();
            }
        }

        private SqliteConnection Connection
        {
            get
            {
                if (_connection is null)
                {
                    _connection = new SqliteConnection($"Data Source={_name}");
                    _connection.Open();
                }
                return _connection;
            }
        }

        public void EnsureCreated()
        {
            ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS Steps (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL,
    Location INTEGER NOT NULL,
    Type TEXT NOT NULL,
    X REAL NOT NULL,
    Y REAL NOT NULL,
    Amount INTEGER NOT NULL,
    Item TEXT NOT NULL,
    Orientation TEXT,
    Inventory TEXT,
    Priority TEXT,
    Modifier TEXT,
    Color TEXT NOT NULL,
    Comment TEXT NOT NULL,
    IsSkip INTEGER NOT NULL
);");

            ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS Settings (
    ""Key"" TEXT NOT NULL PRIMARY KEY,
    ""Value"" TEXT NOT NULL
);");
        }

        public void EnsureDeleted()
        {
            if (_connection is not null)
            {
                _connection.Dispose();
                SqliteConnection.ClearPool(_connection);
                _connection = null;
            }
            File.Delete(_name);
        }

        public void SetupTriggers()
        {
            // Trigger to prevent updating Templates.Type
            ExecuteNonQuery(@"
CREATE TRIGGER IF NOT EXISTS block_update_step_type
BEFORE UPDATE OF TYPE ON Steps
BEGIN
  SELECT RAISE(ABORT, 'Updating Steps.Type is prohibited');
END;
");
        }

        public void RunInTransaction(Action action)
        {
            if (_transaction is not null)
            {
                action();
                return;
            }

            using var transaction = Connection.BeginTransaction();
            _transaction = transaction;
            try
            {
                action();
                transaction.Commit();
            }
            finally
            {
                _transaction = null;
            }
        }

        public int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            using var command = CreateCommand(sql, parameters);
            return command.ExecuteNonQuery();
        }

        public T? ExecuteScalar<T>(string sql, params SqliteParameter[] parameters)
        {
            using var command = CreateCommand(sql, parameters);
            var value = command.ExecuteScalar();
            return value is null or DBNull ? default : (T)Convert.ChangeType(value, typeof(T));
        }

        public List<T> Query<T>(string sql, Func<SqliteDataReader, T> map, params SqliteParameter[] parameters)
        {
            using var command = CreateCommand(sql, parameters);
            using var reader = command.ExecuteReader();
            var results = new List<T>();
            while (reader.Read())
            {
                results.Add(map(reader));
            }
            return results;
        }

        public List<Step> QuerySteps(string filterClause, params SqliteParameter[] parameters)
        {
            return Query($"SELECT {StepSelectColumns} FROM Steps {filterClause}", ReadStep, parameters);
        }

        public List<Step> GetSteps(string stepName)
        {
            return QuerySteps("WHERE Name = $name ORDER BY Location", new SqliteParameter("$name", stepName));
        }

        public void InsertSteps(IEnumerable<Step> steps)
        {
            RunInTransaction(() =>
            {
                foreach (var step in steps)
                {
                    var parameters = ToColumnValues(step)
                        .Select(column => new SqliteParameter($"${column.Key}", column.Value))
                        .ToArray();
                    ExecuteNonQuery(InsertStepSql, parameters);
                }
            });
        }

        public Setting? GetSetting(string key)
        {
            return Query("SELECT \"Key\", \"Value\" FROM \"Settings\" WHERE \"Key\" = $key", ReadSetting, new SqliteParameter("$key", key)).FirstOrDefault();
        }

        public void AddSetting(Setting setting)
        {
            ExecuteNonQuery("INSERT INTO \"Settings\" (\"Key\", \"Value\") VALUES ($key, $value)", new SqliteParameter("$key", setting.Key), new SqliteParameter("$value", setting.Value));
        }

        public void UpdateSetting(string key, string value)
        {
            ExecuteNonQuery("UPDATE \"Settings\" SET \"Value\" = $value WHERE \"Key\" = $key", new SqliteParameter("$value", value), new SqliteParameter("$key", key));
        }

        // Guid text contains only hex digits and dashes, so inlining the values is safe.
        public static string IdInClause(IEnumerable<Guid> ids)
        {
            return string.Join(", ", ids.Select(id => $"'{id.ToString().ToUpperInvariant()}'"));
        }

        internal static Dictionary<string, object?> ToColumnValues(Step step)
        {
            return new()
            {
                ["Id"] = step.Id.ToString().ToUpperInvariant(),
                ["Name"] = step.Name,
                ["Location"] = step.Location,
                ["Type"] = StepTypeExtensions.ToString(step.Type),
                ["X"] = step.X,
                ["Y"] = step.Y,
                ["Amount"] = step.Amount,
                ["Item"] = step.Item,
                ["Orientation"] = OrientationTypeExtensions.ToString(step.Orientation),
                ["Inventory"] = InventoryTypeExtensions.ToString(step.Inventory),
                ["Priority"] = Priority.ToString(step.Priority),
                ["Modifier"] = ModifierTypeExtensions.ToString(step.Modifier),
                ["Color"] = step.Color,
                ["Comment"] = step.Comment,
                ["IsSkip"] = step.IsSkip,
            };
        }

        private SqliteCommand CreateCommand(string sql, SqliteParameter[] parameters)
        {
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = _transaction;
            command.Parameters.AddRange(parameters);
            return command;
        }

        private static Step ReadStep(SqliteDataReader reader)
        {
            return new Step
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = ReadText(reader, 1),
                Location = reader.GetInt32(2),
                Type = StepTypeExtensions.FromString(ReadText(reader, 3)),
                X = reader.GetDouble(4),
                Y = reader.GetDouble(5),
                Amount = reader.GetInt32(6),
                Item = ReadText(reader, 7),
                Orientation = OrientationTypeExtensions.FromString(ReadText(reader, 8)),
                Inventory = InventoryTypeExtensions.FromString(ReadText(reader, 9)),
                Priority = Priority.FromString(ReadText(reader, 10)),
                Modifier = ModifierTypeExtensions.FromString(ReadText(reader, 11)),
                Color = ReadText(reader, 12),
                Comment = ReadText(reader, 13),
                IsSkip = reader.GetInt64(14) != 0,
            };
        }

        private static Setting ReadSetting(SqliteDataReader reader)
        {
            return new Setting
            {
                Key = reader.GetString(0),
                Value = reader.GetString(1),
            };
        }

        private static string ReadText(SqliteDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}