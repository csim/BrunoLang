namespace Bruno.Compiler
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Bruno.Compiler.Constants;
    using Bruno.Exceptions;
    using static Constants.Punctuation;
    using static Constants.Characters;

    internal class LexService
    {
        public LexService([NotNull] InputService input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        private readonly InputService _input;
        private          LexToken     _peekCache;

        public bool IsEnd()
            => Peek() == null;

        public LexToken Next()
        {
            LexToken ret = _peekCache ?? ReadNext();
            _peekCache = null;
            //Console.WriteLine($"{_input.Location} Next: {ret}");
            return ret;
        }

        public LexToken Peek()
            => _peekCache ??= ReadNext();

        private static bool IsDigit(char ch)
            => Digits.All.Contains(ch);

        private static bool IsIdentifier(char ch)
            => Regex.IsMatch(ch.ToString(), @"[A-Za-z_0123456789]");

        private bool IsNextDigit()
            => IsDigit(_input.Peek());

        private bool IsNextIdentifierStart()
            => Regex.IsMatch(_input.Peek().ToString(), "[A-Za-z_]");

        private bool IsNextOperator()
            => IsOperator(_input.Peek());

        private bool IsNextPunctuation()
            => IsPunctuation(_input.Peek());

        private bool IsNextPunctuation(char ch)
            => IsNextPunctuation() && _input.Peek() == ch;

        private static bool IsOperator(char ch)
            => Operators.All.Contains(ch);

        private static bool IsPunctuation(char ch)
            => All.Contains(ch);

        private static bool IsWhitespace(char ch)
            => WhiteSpace.All.Contains(ch);

        private LexToken ReadDouble()
        {
            if (!IsNextDigit()) return null;

            bool hasDot = false;
            string number = ReadWhile(ch => {
                                          if (ch != Period) return IsDigit(ch);
                                          if (hasDot) return false;

                                          return hasDot = true;
                                      });

            return new LexToken(LexTokenType.DoubleLiteral, double.Parse(number));
        }

        private LexToken ReadIdentifier()
        {
            if (!IsNextIdentifierStart()) return null;

            string id = ReadWhile(IsIdentifier);
            return new LexToken(LexTokenType.Identifier, id);
        }

        private LexToken ReadNext()
        {
            ReadWhile(IsWhitespace);

            if (_input.IsEnd()) return null;

            SkipComment();

            return ReadDouble()
                   ?? ReadString()
                   ?? ReadPunctuation()
                   ?? ReadIdentifier()
                   ?? ReadOperator()
                   ?? throw new Exception($"Unexpected character: {_input.Next()}  {_input.Location}");
        }

        private LexToken ReadOperator()
            => !IsNextOperator() ? null : new LexToken(LexTokenType.Operator, _input.Next().ToString());

        private LexToken ReadPunctuation()
            => !IsNextPunctuation() ? null : new LexToken(LexTokenType.Punctuation, _input.Next().ToString());

        private LexToken ReadString()
        {
            if (!IsNextPunctuation(DoubleQuote)) return null;

            StringBuilder ret = new();
            char          end = DoubleQuote;

            _input.Next();

            while (!_input.IsEnd())
            {
                char ch = _input.Next();

                if (ch == end && ch != Backslash) break;

                ret.Append(ch);
            }

            return new LexToken(LexTokenType.StringLiteral, ret.ToString());
        }

        private string ReadWhile(Func<char, bool> condition)
        {
            StringBuilder ret = new();

            while (!_input.IsEnd() && condition(_input.Peek())) ret.Append(_input.Next());

            return ret.ToString();
        }

        private void SkipComment()
        {
            if (_input.Peek() != HashSign) return;

            ReadWhile(ch => ch != Linefeed);
            _input.Next();
        }
    }

    internal record LexToken(LexTokenType Type, object Value)
    {
        public override string ToString()
        {
            string val = Value is string ? $"\"{Value}\"" : Value.ToString();
            return $"Token: {Type}={val}";
        }

        public char ValueAsChar()
        {
            string sval = Value?.ToString() ?? throw new BrunoException("Null value for token.");
            if (sval.Length != 1) throw new BrunoException($"Expected single character. ({sval})");

            return sval[0];
        }
    }

    internal enum LexTokenType
    {
        DoubleLiteral,
        Identifier,
        Operator,
        Punctuation,
        StringLiteral
    }
}