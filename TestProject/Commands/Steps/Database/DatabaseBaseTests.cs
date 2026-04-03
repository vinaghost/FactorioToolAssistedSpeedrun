namespace TestProject.Commands.Steps.Database
{
    public class DatabaseBaseTests : IClassFixture<DatabaseFixture>
    {
        protected const string StepCategoryName = "";
        protected const int Amount = 20;

        protected readonly DatabaseFixture _fixture;

        public DatabaseBaseTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }
    }
}