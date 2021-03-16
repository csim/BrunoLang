namespace Bruno.Tests
{
    using System;
    using Bruno.Compiler;
    using Xunit;
    using Xunit.Abstractions;

    public class UnitTest1
    {
        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        [Fact]
        public void Test1()
        {
            var content = $"x = 1 + 2{Environment.NewLine}print(x)";
            var program = ParseService.Parse(content);

            Assert.NotNull(program);

            WriteLine(program.ToString());
            WriteLine("");

            WriteLine(program.Evaluate()?.ToString() ?? "<null>");
        }

        protected void WriteLine(string content)
        {
            _testOutputHelper.WriteLine(content);
        }
    }
}