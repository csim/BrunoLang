namespace Bruno.Compiler.Constants
{
    public static class WhiteSpace
    {
        public static readonly char[] All = {
                                                CarriageReturn,
                                                Tab,
                                                Space
                                            };
        public const char CarriageReturn = '\r';
        public const char Space = ' ';
        public const char Tab = '\t';
    }
}