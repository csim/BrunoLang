namespace Bruno.Compiler
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class ParserLexer
    {
        public ParserLexer([NotNull] InputStream input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        private readonly char[] _digits =
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9'
        };
        private readonly InputStream _input;
        private readonly char[] _operators
            =
            {
                '+',
                '-',
                '*',
                '/'
                //'%',
                //'=',
                //'&',
                //'|',
                //'<',
                //'>',
                //'!'
            };

        private LexToken _peekCache;

        private readonly char[] _punctuation =
        {
            '"',
            '.',
            ',',
            '(',
            ')',
            '{',
            '}'
        };

        private readonly char[] _whitespace =
        {
            ' ',
            '\n',
            '\r',
            '\t'
        };

        public bool IsEnd()
            => Peek() == null;

        public LexToken Next()
        {
            var ret = _peekCache ?? ReadNext();
            _peekCache = null;
            //Console.WriteLine($"{_input.Location} Next: {ret}");
            return ret;
        }

        public LexToken Peek()
            => _peekCache ??= ReadNext();

        private bool IsDigit(char ch)
            => _digits.Contains(ch);

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

        private bool IsOperator(char ch)
            => _operators.Contains(ch);

        private bool IsPunctuation(char ch)
            => _punctuation.Contains(ch);

        private bool IsWhitespace(char ch)
            => _whitespace.Contains(ch);

        private LexToken ReadDouble()
        {
            if (!IsNextDigit()) return null;

            var hasDot = false;
            var number = ReadWhile(ch =>
                                   {
                                       if (ch == '.')
                                       {
                                           if (hasDot) return false;

                                           hasDot = true;
                                           return true;
                                       }

                                       return IsDigit(ch);
                                   });

            return new LexToken(LexTokenType.DoubleLiteral, double.Parse(number));
        }

        private LexToken ReadIdentifier()
        {
            if (!IsNextIdentifierStart()) return null;

            var id = ReadWhile(IsIdentifier);
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
        {
            if (!IsNextOperator()) return null;

            return new LexToken(LexTokenType.Operator, _input.Next().ToString());
        }

        private LexToken ReadPunctuation()
        {
            if (!IsNextPunctuation()) return null;

            return new LexToken(LexTokenType.Punctuation, _input.Next().ToString());
        }

        private LexToken ReadString()
        {
            if (!IsNextPunctuation('"')) return null;

            var ret = new StringBuilder();
            var end = '"';

            _input.Next();

            while (!_input.IsEnd())
            {
                var ch = _input.Next();

                if (ch == end && ch != '\\') break;

                ret.Append(ch);
            }

            return new LexToken(LexTokenType.StringLiteral, ret.ToString());
        }

        private string ReadWhile(Func<char, bool> condition)
        {
            var ret = new StringBuilder();

            while (!_input.IsEnd() && condition(_input.Peek())) ret.Append(_input.Next());

            return ret.ToString();
        }

        private void SkipComment()
        {
            if (_input.Peek() != '#') return;

            ReadWhile(ch => ch != '\n');
            _input.Next();
        }
    }

    internal class LexToken
    {
        public LexToken(LexTokenType type, object value)
        {
            Type  = type;
            Value = value is char ? value.ToString() : value;
        }

        public LexTokenType Type { get; }

        public object Value { get; }

        public override string ToString()
        {
            var val = Value is string ? $"\"{Value}\"" : Value.ToString();
            return $"Token: {Type}={val}";
        }
    }

    internal enum LexTokenType
    {
        DoubleLiteral,
        Identifier,
        Operator,
        Comment,
        Punctuation,
        StringLiteral
    }
}