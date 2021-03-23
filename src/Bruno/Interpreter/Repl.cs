namespace Bruno.Interpreter
{
    using System;
    using Bruno.Compiler;
    using Bruno.Compiler.Ast;
    using Bruno.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Repl
    {
        private string _command;
        private JObject _context;
        private OutputService _output;

        public void Run()
        {
            Console.CancelKeyPress += CancelKeyPress;

            using (_output = new OutputService(true, true))
            {
                _context = JObject.FromObject(new { input1 = "555-99-5656" });
                PrintContext();

                while (true)
                {
                    try
                    {
                        Console.Write(":> ");
                        _command = Console.ReadLine();
                        _output.WriteLine($"Command: {_command}");

                        if (_context == null)
                        {
                            throw new ApplicationException("Context is empty.");
                        }

                        if (string.IsNullOrEmpty(value: _command))
                        {
                            _command = "context";
                        }

                        if (_command.ToLower() == "exit")
                        {
                            break;
                        }

                        if (_command.StartsWith("{") || _command.ToLower().StartsWith("context {"))
                        {
                            _context = JObject.Parse(_command.Replace("context ", ""));
                            PrintContext();
                            continue;
                        }

                        if (_command.ToLower().StartsWith("context"))
                        {
                            PrintContext();
                            continue;
                        }

                        try
                        {
                            // Evaluate
                            //var contextJson = _context.ToString();
                            //var parameters = new FormulaRuntimeParameters(contextJson);
                            //var schema = Schema.GetSchemaFromJson(contextJson);

                            //var expr = new FormulaWithParameters(_command, schema);
                            //var value = await runner.RunAsync(expr, parameters);

                            BrunoProgram ast    = ParseService.Parse(raw: _command);
                            object       result = InterpreterService.Evaluate(expression: ast);

                            PrintResult(value: result);
                            _command = null;
                        }
                        catch (BrunoRuntimeException error)
                        {
                            PrintException(ex: error);
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintException(ex: ex);
                    }
                }
            }
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
            => _command = "exit";

        private void PrintContext()
        {
            _output.WriteLine("");
            _output.WriteLine("Context:");
            _output.WriteLine(_context?.ToString());
            _output.WriteLine("");
        }

        private void PrintException(Exception ex)
        {
            _output.WriteLine("");
            _output.WriteLine("Exception:");
            _output.WriteLine(target: ex);
        }

        private void PrintResult(object value)
        {
            PrintContext();
            string json = JsonConvert.SerializeObject(value: value);
            _output.WriteLine("Result:");
            _output.WriteLine($"{json} <{value?.GetType().FullName}>");
            _output.WriteLine("");
        }
    }
}