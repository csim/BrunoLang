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

        private readonly bool         _consoleEnabled;
        private readonly bool         _txtEnabled;
        private          StreamWriter _txtWriter;

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
                Console.WriteLine(text);
            }

            if (_txtEnabled)
            {
                WriteTextFile(text);
            }
        }

        private void Flush()
            => _txtWriter?.Flush();

        private void WriteTextFile(string text)
        {
            if (_txtWriter == null)
            {
                string basePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location) ?? ".", "output");

                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string baseFilename = Path.Combine(basePath, $"{GetType().Namespace}-{DateTime.Now:yyyyMMdd_HHmmss}");

                string txtFilename = $"{baseFilename}.txt";

                if (_txtEnabled)
                {
                    _txtWriter = new StreamWriter(txtFilename, false);
                    WriteLine($"Writing: {txtFilename}");
                }
            }

            _txtWriter?.WriteLine(text);
            Flush();
        }
    }
}