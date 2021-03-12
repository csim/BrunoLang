namespace Bruno
{
    using System;
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
               .ParseArguments<Options>(args)
               .WithParsed(options => new Program(options).Execute());

        private void Execute()
        {
            if (_options.Verbose)
            {
                Console.WriteLine($"Verbose output enabled. Current Arguments: -v {_options.Verbose}");
                Console.WriteLine("Quick Start Example! App is in Verbose mode!");
            }
            else
            {
                Console.WriteLine($"Current Arguments: -v {_options.Verbose}");
                Console.WriteLine("Quick Start Example!");
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }
    }
}