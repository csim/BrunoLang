namespace Bruno.Tests
{
    using Xunit.Abstractions;

    public class TestBase
    {
        protected TestBase(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private readonly ITestOutputHelper _outputHelper;

        protected void WriteLine(string content)
            => _outputHelper.WriteLine(content);
    }
}