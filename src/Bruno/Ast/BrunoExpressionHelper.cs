namespace Bruno.Ast
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal static class BrunoExpressionHelper
    {
        public static BrunoExpression Accessor(string name)
            => new BrunoAccessor(name);

        public static BrunoExpression Concatenate(BrunoExpression left, BrunoExpression right)
            => new BrunoFunc(nameof(Concatenate),
                             new[]
                             {
                                 left,
                                 right
                             });

        public static BrunoExpression DateTimeValue(BrunoExpression value, BrunoExpression locale = null)
            => new BrunoFunc(nameof(DateTimeValue),
                             new[]
                             {
                                 value,
                                 locale
                             });

        public static BrunoExpression DateValue(BrunoExpression value, BrunoExpression locale = null)
            => new BrunoFunc(nameof(DateValue),
                             new[]
                             {
                                 value,
                                 locale
                             });

        public static BrunoExpression Divide(BrunoExpression left, BrunoExpression right)
        {
            if (right is BrunoNumber { Value: 1 }) return left;

            return new BrunoDivide(left, right);
        }

        public static BrunoExpression Dot(BrunoExpression subject, string accessor)
            => new BrunoDot(subject, Accessor(accessor));

        public static BrunoExpression Dot(BrunoExpression subject, BrunoExpression accessor)
            => new BrunoDot(subject, accessor);

        public static BrunoExpression DoubleLiteral(double value)
            => new BrunoNumber(value);

        public static string EscapeStringLiteral(string str)
            => str == null ? null : $"\"{str.Replace("\"", "\"\"")}\"";

        public static BrunoExpression Find(BrunoExpression needle, BrunoExpression haystack)
            => new BrunoFunc(nameof(Find),
                             new[]
                             {
                                 needle,
                                 haystack
                             });

        public static BrunoExpression First(BrunoExpression list)
            => new BrunoFunc(nameof(First), new[] { list });

        public static BrunoExpression FirstN(BrunoExpression list, BrunoExpression count)
            => new BrunoFunc(nameof(FirstN),
                             new[]
                             {
                                 list,
                                 count
                             });

        public static BrunoExpression FuncApp(string name, IEnumerable<BrunoExpression> arguments)
            => new BrunoFunc(name, arguments);

        public static BrunoExpression Last(BrunoExpression list)
            => new BrunoFunc(nameof(Last),
                             new[] { list });

        public static BrunoExpression LastN(BrunoExpression list, BrunoExpression num)
            => new BrunoFunc(nameof(LastN),
                             new[]
                             {
                                 list,
                                 num
                             });

        public static BrunoExpression Left(BrunoExpression str, BrunoExpression numChars)
            => new BrunoFunc(nameof(Left),
                             new[]
                             {
                                 str,
                                 numChars
                             });

        public static BrunoExpression Len(BrunoExpression str)
            => new BrunoFunc(nameof(Len),
                             new[] { str });

        public static BrunoExpression Lower(BrunoExpression str)
            => new BrunoFunc(nameof(Lower),
                             new[] { str });

        public static BrunoExpression Match(BrunoExpression str, BrunoExpression regex)
            => new BrunoFunc(nameof(Match),
                             new[]
                             {
                                 str,
                                 regex
                             });

        public static BrunoExpression MatchAll(BrunoExpression str, BrunoExpression regex)
            => new BrunoFunc(nameof(MatchAll),
                             new[]
                             {
                                 str,
                                 regex
                             });

        public static BrunoExpression Mid(BrunoExpression str,
                                          BrunoExpression start,
                                          BrunoExpression numChars = null)
            => new BrunoFunc(nameof(Mid),
                             new[]
                             {
                                 str,
                                 start,
                                 numChars
                             });

        public static BrunoExpression Minus(BrunoExpression left, BrunoExpression right)
        {
            if (right is BrunoNumber { Value: 0 }) return left;

            return new BrunoMinus(left, right);
        }

        public static BrunoExpression Minus1(BrunoExpression val)
        {
            if (val is BrunoNumber doubleLiteral) return DoubleLiteral(doubleLiteral.Value - 1);

            if (val is BrunoPlus { Right: BrunoNumber plusRight2 } plus2)
            {
                var newRight = plusRight2.Value - 1;
                return newRight == 0 ? plus2.Left : Plus(plus2.Left, DoubleLiteral(newRight));
            }

            if (val is BrunoMinus { Right: BrunoNumber minusRight2 } minus2)
            {
                return Minus(minus2.Left, DoubleLiteral(minusRight2.Value + 1));
            }

            return Minus(val, DoubleLiteral(1));
        }

        public static BrunoExpression Multiply(BrunoExpression left, BrunoExpression right)
        {
            if (right is BrunoNumber { Value: 1 }) return left;

            if (left is BrunoNumber { Value: 1 }) return right;


            return new BrunoMultiply(left, right);
        }

        public static BrunoExpression Nth(BrunoExpression list, BrunoExpression n)
            => Last(FirstN(list, n));

        public static BrunoExpression NthFromEnd(BrunoExpression list, BrunoExpression n)
            => First(LastN(list, n));

        public static BrunoExpression Parenthesis(BrunoExpression body)
            => new BrunoParenthesis(body);

        public static BrunoExpression Plus(BrunoExpression left, BrunoExpression right)
        {
            if (right is BrunoNumber { Value: 0 }) return left;

            if (right is BrunoNumber { Value: 0 }) return right;

            return new BrunoPlus(left, right);
        }

        public static BrunoExpression Plus1(BrunoExpression val)
        {
            if (val is BrunoNumber doubleLiteral) return DoubleLiteral(doubleLiteral.Value + 1);

            if (val is BrunoPlus { Right: BrunoNumber iPlus1 } plus2)
            {
                var newDouble = iPlus1.Value + 1;
                return newDouble == 0 ? plus2.Left : Plus(plus2.Left, DoubleLiteral(newDouble));
            }

            if (val is BrunoMinus { Right: BrunoNumber iMinus1 } minus2)
            {
                double newDouble = iMinus1.Value - 1;
                return newDouble == 0 ? minus2.Left : Plus(minus2.Left, DoubleLiteral(newDouble));
            }

            return Plus(val, DoubleLiteral(1));
        }

        public static BrunoExpression Proper(BrunoExpression str)
            => new BrunoFunc(nameof(Proper),
                             new[] { str });

        public static BrunoExpression Right(BrunoExpression str, BrunoExpression numChars)
            => new BrunoFunc(nameof(Right),
                             new[]
                             {
                                 str,
                                 numChars
                             });

        public static BrunoExpression Round(BrunoExpression value, BrunoExpression decimals)
            => new BrunoFunc(nameof(Round),
                             new[]
                             {
                                 value,
                                 decimals
                             });

        public static BrunoExpression RoundDown(BrunoExpression value, BrunoExpression decimals)
            => new BrunoFunc(nameof(RoundDown),
                             new[]
                             {
                                 value,
                                 decimals
                             });

        public static BrunoExpression RoundUp(BrunoExpression value, BrunoExpression decimals)
            => new BrunoFunc(nameof(RoundUp),
                             new[]
                             {
                                 value,
                                 decimals
                             });

        public static BrunoExpression Split(BrunoExpression str, BrunoExpression delimiter)
            => new BrunoFunc(nameof(Split),
                             new[]
                             {
                                 str,
                                 delimiter
                             });

        public static BrunoExpression StringLiteral(string value)
            => new BrunoString(value);

        public static BrunoExpression Text(BrunoExpression value, BrunoExpression format)
            => new BrunoFunc(nameof(Text),
                             new[]
                             {
                                 value,
                                 format
                             });

        public static BrunoExpression Time(BrunoExpression value, BrunoExpression format)
            => new BrunoFunc(nameof(Time),
                             new[]
                             {
                                 value,
                                 format
                             });

        public static BrunoExpression TimeValue(BrunoExpression value)
            => new BrunoFunc(nameof(TimeValue),
                             new[] { value });

        public static string ToRegexString(Regex regex)
            => regex.ToString();

        public static BrunoExpression TrimEnds(BrunoExpression value)
            => new BrunoFunc(nameof(TrimEnds),
                             new[] { value });

        public static BrunoExpression Upper(BrunoExpression str)
            => new BrunoFunc(nameof(Upper),
                             new[] { str });

        public static BrunoExpression Value(BrunoExpression value)
            => new BrunoFunc(nameof(Value),
                             new[] { value });

        public static BrunoVariable Variable(string name)
            => new BrunoVariable(name);

        public static BrunoExpression With(IDictionary<string, BrunoExpression> context, BrunoExpression body)
            => With((IReadOnlyDictionary<string, BrunoExpression>)context, body);

        public static BrunoExpression With(IReadOnlyDictionary<string, BrunoExpression> context, BrunoExpression body)
            => new BrunoWith(context, body);
    }
}