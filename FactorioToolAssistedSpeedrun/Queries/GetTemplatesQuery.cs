namespace FactorioToolAssistedSpeedrun.Queries
{
    public class GetTemplatesQuery
    {
        public required string ProjectDataFile { get; init; }

        public List<string> Execute()
        {
            using var context = new ProjectDbContext(ProjectDataFile);
            return context.Query("SELECT DISTINCT Name FROM Steps WHERE Name IS NOT NULL AND Name <> '' ORDER BY Name", reader => reader.GetString(0));
        }
    }
}