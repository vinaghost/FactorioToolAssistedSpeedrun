namespace TestProject.Commands.Steps.UI
{
    public class UIBaseTests : IClassFixture<UIFixture>
    {
        protected const int Amount = 5;

        protected readonly UIFixture _fixture;

        public UIBaseTests(UIFixture fixture)
        {
            _fixture = fixture;
        }
    }
}