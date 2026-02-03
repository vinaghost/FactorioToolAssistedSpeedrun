namespace TestProject.Commands.Steps.UI
{
    [CollectionDefinition("Step model")]
    public class StepModelCollection : ICollectionFixture<UIFixture>
    {
    }

    public class UIBaseTests
    {
        protected const int Amount = 5;

        protected readonly UIFixture _fixture;

        public UIBaseTests(UIFixture fixture)
        {
            _fixture = fixture;
        }
    }
}