namespace Bruno.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Bruno.Compiler;
    using Bruno.Compiler.Ast;
    using Xunit;
    using Xunit.Abstractions;

    public class AutoTest
    {
        public AutoTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        [Theory]
        [ClassData(typeof(AutoTestGenerator))]
        public void Parse(AutoTestInfo info)
        {
            string       content = ResourceUtil.GetContent(info.ResourcePath);
            BrunoProgram program = ParseService.Parse(content);
            Assert.NotNull(program);

            WriteLine(info.Filename);

            WriteLine(new string('-', info.Filename.Length));
            WriteLine(program.ToString());
        }

        private void WriteLine(string content)
            => _testOutputHelper.WriteLine(content);
    }

    public class AutoTestGenerator : IEnumerable<object[]>
    {
        private List<AutoTestInfo[]> _data;

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerator<object[]> GetEnumerator()
        {
            if (_data == null) Load();
            return _data.GetEnumerator();
        }

        private void Load()
        {
            string              rootPath       = $"{GetType().Namespace}.Content";
            string              filenamePrefix = $"{rootPath}.";
            IEnumerable<string> contentPaths   = ResourceUtil.FindByPrefix(rootPath);

            _data = new List<AutoTestInfo[]>();

            foreach (string contentPath in contentPaths)
            {
                string filename    = contentPath.Replace(filenamePrefix, "");
                string displayName = filename.Replace(".bruno", "");
                _data.Add(new[] {
                                    new AutoTestInfo(filename, displayName, contentPath)
                                });
            }
        }
    }

    [DebuggerDisplay(nameof(Filename))]
    public record AutoTestInfo(string Filename, string DisplayName, string ResourcePath)
    {
        public override string ToString()
            => DisplayName;
    }
}