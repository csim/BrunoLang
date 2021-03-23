namespace Bruno.Tests
{
    using Bruno.Compiler;
    using Bruno.Compiler.Ast;
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
            //var content = @"x = 1 + 2
            //                print(x)";

            //string content = @"if 1 == 1;
            //                print(""true"");";


            string[] contents = {
                                    "(1 + 2) * 3",
                                    "1 + 2 * 3",
                                    "1 + 4 * 2 - 3",
                                    "1 + 2 * 3 - a(1, u(2))",
                                    "((1 + 2) + 3) + a(1, u(2))",
                                    "1 + 2 * 3 - a(1, u(2))",
                                    "1 + 1",
                                    "1 + 2 * 3 - 4",
                                    "1 + 2 * 3 - a(1, u(2))",
                                    "1 + Get(1, 2, a(22, z))",
                                    "12.3 + add(1)",
                                    "regex.StartMatch",
                                    "\"yo\".ToString()",
                                    "add()",
                                    "add().yo() + add(1, 3 + 2).start(x, 7 * 8)",
                                    "add().yo(1, 2, 3, 5 * 88)",
                                    "add(2, 5).yo(1, 2, 32, 5).Start(x, 1 + 2)",
                                    "add(1).yo(1).start(3, 4)",
                                    "\"string\".Start",
                                    "\"string\".Start(1 + 2, \"ya\").Do(x).No()",
                                    "(1 + 2).ToString",
                                    "var1",
                                    "\"string\"",
                                    "a.ToString + 2",
                                    "i + j"
                                    //"add(i)",
                                    //"(i1(2) + 12)",
                                    //"add(a(a(i + l) + 2, z, k), j)",
                                    //"Len( Match(\"string1\", 14).StartMatch)",
                                    //"Value(i1, i2).Start + 1",
                                };


            int i = 1;
            //BrunoExpression exp = default;
            //foreach (string formula in _baselineFormulas) {
            foreach (string content in contents)
            {
                BrunoProgram program = ParseService.Parse(raw: content);
                Assert.NotNull(@object: program);

                //object result = program.Evaluate();

                WriteLine(program.ToString());
                WriteLine("");

                //WriteLine(result?.ToString() ?? "<null>");

                //try
                //{
                //    exp = Parser.Parse(formula);
                //    Assert.AreEqual(formula.Replace(" ", ""), exp.ToString().Replace(" ", ""));
                //    i++;
                //    if (formulas.Length < 100)
                //    {
                //        WriteOutput(formula);
                //    }
                //}
                //catch (Exception)
                //{
                //    WriteOutput(formula);
                //    if (formulas.Length < 100)
                //    {
                //        throw;
                //    }
                //}
            }
        }

        //void WriteOutput(string content)
        //{
        //    WriteLine($"{i}: ===========================");
        //    WriteLine(exp);
        //    WriteLine(content);
        //    WriteLine($"--------------------------------");
        //    WriteLine("");

        //}

        private void WriteLine(string content)
            => _testOutputHelper.WriteLine(message: content);
    }
}