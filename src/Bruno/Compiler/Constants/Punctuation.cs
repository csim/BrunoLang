namespace Bruno.Compiler.Constants
{
    public static class Punctuation
    {
        public static readonly char[] All = {
                                                CloseBracket,
                                                CloseParen,
                                                Colon,
                                                Colon,
                                                Comma,
                                                DoubleQuote,
                                                Linefeed,
                                                OpenBracket,
                                                OpenParen,
                                                Period,
                                                SemiColon
                                            };
        public const char CloseBracket = '.';
        public const char CloseParen   = ')';
        public const char Colon        = ':';
        public const char Comma        = ',';
        public const char DoubleQuote  = '"';
        public const char Linefeed     = '\n';
        public const char OpenBracket  = '.';
        public const char OpenParen    = '(';
        public const char Period       = '.';
        public const char SemiColon    = ';';
    }
}