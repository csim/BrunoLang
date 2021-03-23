namespace Bruno
{
    using System;
    using System.IO;
    using Bruno.Ast;
    using Bruno.Compiler;
    using Bruno.Interpreter;
    using CommandLine;

    internal class Program
    {
        private Program(Options options)
        {
            _options = options;
        }

        private readonly Options _options;

        public static void Main(string[] args)
            => Parser
               .Default
               .ParseArguments<Options>(args: args)
               .WithParsed(options => new Program(options: options).Execute());

        private void Execute()
        {
            if (_options.Interactive)
            {
                Repl repl = new();
                repl.Run();
                return;
            }

            string content = File.ReadAllText(@"C:\Source\BrunoLang\Samples\Example1.bruno");
            //var content = "x = 1 + 2";
            BrunoProgram program = ParseService.Parse(raw: content);
            //Console.WriteLine(content);
            //Console.WriteLine("---");
            //Console.WriteLine(program);
            //Console.WriteLine("---");
            Console.WriteLine(program.Evaluate()?.ToString());

            //if (_options.Verbose)
            //{
            //    Console.WriteLine($"Verbose output enabled. Current Arguments: -v {_options.Verbose}");
            //    Console.WriteLine("Quick Start Example! App is in Verbose mode!");
            //}
            //else
            //{
            //    Console.WriteLine($"Current Arguments: -v {_options.Verbose}");
            //    Console.WriteLine("Quick Start Example!");
            //}
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Options
        {
            [Option('i', "interactive", Required = false, HelpText = "Run interactive command prompt.")]
            public bool Interactive { get; set; }
        }
    }
}