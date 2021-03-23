namespace Bruno
{
    using System;
    using System.IO;

    public class OutputService : IDisposable
    {
        public OutputService(bool consoleEnabled, bool txtEnabled)
        {
            _consoleEnabled = consoleEnabled;
            _txtEnabled     = txtEnabled;
        }

        private readonly bool _consoleEnabled;
        private readonly bool _txtEnabled;
        private StreamWriter _txtWriter;

        public void Dispose()
        {
            Flush();

            _txtWriter?.Dispose();
            _txtWriter = null;
        }

        public void WriteLine(object target)
            => WriteLine(target.ToString());

        public void WriteLine(string text)
        {
            if (_consoleEnabled)
            {
                Console.WriteLine(value: text);
            }

            if (_txtEnabled)
            {
                WriteTextFile(text: text);
            }
        }

        private void Flush()
            => _txtWriter?.Flush();

        private void WriteTextFile(string text)
        {
            if (_txtWriter == null)
            {
                string basePath = Path.Combine(Path.GetDirectoryName(path: GetType().Assembly.Location) ?? ".", "output");

                if (!Directory.Exists(path: basePath))
                {
                    Directory.CreateDirectory(path: basePath);
                }

                string baseFilename = Path.Combine(path1: basePath, $"{GetType().Namespace}-{DateTime.Now:yyyyMMdd_HHmmss}");

                string txtFilename = $"{baseFilename}.txt";

                if (_txtEnabled)
                {
                    _txtWriter = new StreamWriter(path: txtFilename, false);
                    WriteLine($"Writing: {txtFilename}");
                }
            }

            _txtWriter?.WriteLine(value: text);
            Flush();
        }
    }
}