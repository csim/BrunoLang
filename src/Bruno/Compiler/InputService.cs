namespace Bruno.Compiler
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using static Constants.Punctuation;

    internal class InputService
    {
        public InputService([NotNull] string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("Value cannot be null or empty.", nameof(input));

            _input = input.ToCharArray();
        }

        private readonly char[] _input;
        private          int    _position;

        public string Location => $"[{Line}, {Column}]";

        private int Column { get; set; } = 1;

        private int Line { get; set; } = 1;

        public bool IsEnd()
            => _position > _input.Length - 1;

        public char Next()
        {
            char ret = _input[_position++];

            if (ret == Linefeed)
            {
                Line++;
                Column = 1;
            }
            else
            {
                Column++;
            }

            return ret;
        }

        public char Peek()
            => _input[_position];
    }
}