namespace Bruno.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Bruno.Compiler.Ast;
    using Bruno.Compiler.Constants;
    using static Ast.BrunoExpressionHelper;
    using static Constants.Punctuation;

    public class ParseService
    {
        private ParseService([NotNull] string formula)
        {
            if (string.IsNullOrEmpty(formula)) throw new ArgumentException("Value cannot be null or empty.", nameof(formula));

            _input = new InputService(formula);
            _lexer = new LexService(_input);
        }

        private readonly InputService _input;
        private readonly LexService _lexer;
        private readonly IDictionary<string, int> _precedence = new Dictionary<string, int> {
                                                                                                { "=", 1 },
                                                                                                { ".", 1 },
                                                                                                { "||", 2 },
                                                                                                { "&&", 3 },
                                                                                                { "<", 7 },
                                                                                                { ">", 7 },
                                                                                                { "<=", 7 },
                                                                                                { ">=", 7 },
                                                                                                { "==", 7 },
                                                                                                { "!=", 7 },
                                                                                                { "+", 10 },
                                                                                                { "-", 10 },
                                                                                                { "*", 20 },
                                                                                                { "/", 20 },
                                                                                                { "%", 20 }
                                                                                            };

        public static BrunoProgram Parse(string raw)
            => new ParseService(raw).ParseProgram();

        private bool IsNextIdentifier()
            => _lexer.Peek()?.Type == LexTokenType.Identifier;

        private bool IsNextLiteral()
        {
            LexToken token = _lexer.Peek();

            return token != null
                   && (token.Type == LexTokenType.StringLiteral || token.Type == LexTokenType.DoubleLiteral);
        }

        private bool IsNextOperator()
            => _lexer.Peek()?.Type == LexTokenType.Operator;

        private bool IsNextPunctuation(char ch)
        {
            LexToken token = _lexer.Peek();

            return token                     != null
                   && token.Type             == LexTokenType.Punctuation
                   && token.Value.ToString() == ch.ToString();
        }

        private IEnumerable<BrunoExpression> ParseDelimited(char begin, char separator, char end)
        {
            List<BrunoExpression> ret = new();

            SkipPunctuation(begin);

            while (!_lexer.IsEnd())
            {
                if (IsNextPunctuation(end)) break;

                BrunoExpression argExp = ParseExpression();
                ret.Add(argExp);

                if (IsNextPunctuation(separator)) SkipPunctuation(separator);
            }

            SkipPunctuation(end);

            return ret;
        }

        private BrunoExpression ParseDot(BrunoExpression left)
        {
            if (!IsNextPunctuation(Period)) return left;

            LexToken token = _lexer.Next();

            if (!IsNextIdentifier()) throw new Exception($"Unexpected token after dot: {token} {_input.Location}");

            BrunoExpression accessor = ParseExpression();
            return Dot(left, accessor);
        }

        private BrunoExpression ParseExpression()
        {
            BrunoExpression ret = ParseLiteral()
                                  ?? ParseGrouping()
                                  ?? ParseIdentifier()
                                  ?? throw new Exception($"Unexpected token: {_lexer.Peek()?.ToString() ?? "<null>"} {_input.Location}");
            ret = ParseDot(ret);
            return ParseOperator(ret);
        }

        private BrunoExpression ParseGrouping()
        {
            if (!IsNextPunctuation(OpenParen)) return null;

            _lexer.Next();
            BrunoExpression ret = Parenthesis(ParseExpression());
            SkipPunctuation(CloseParen);

            return ret;
        }

        private BrunoExpression ParseIdentifier()
        {
            if (!IsNextIdentifier()) return null;

            LexToken token = _lexer.Next();

            if (IsNextPunctuation(OpenParen))
            {
                IEnumerable<BrunoExpression> args = ParseDelimited(OpenParen, Comma, CloseParen);
                return FuncApp(token.Value.ToString(), args);
            }

            return Variable(token.Value.ToString());
        }

        private BrunoExpression ParseLiteral()
        {
            if (!IsNextLiteral()) return null;

            LexToken token = _lexer.Next();

            return token.Type switch {
                       LexTokenType.StringLiteral => StringLiteral(token.Value.ToString()),
                       LexTokenType.DoubleLiteral => DoubleLiteral((double)token.Value),
                       _                          => throw new Exception($"Unexpected token: {token} {_input.Location}")
                   };
        }

        private BrunoExpression ParseOperator(BrunoExpression left)
        {
            if (!IsNextOperator()) return left;

            char op = _lexer.Next().ValueAsChar();

            //var hisPrecedence = _precedence[op];
            //if (hisPrecedence <= myPrecedence) {
            //    return left;
            //}

            BrunoExpression right = ParseExpression();

            return op switch {
                       Operators.Equal        => Assign(left, right),
                       Operators.Plus         => Plus(left, right),
                       Operators.Minus        => Minus(left, right),
                       Operators.Asterisk     => Multiply(left, right),
                       Operators.ForwardSlash => Divide(left, right),
                       _                      => throw new Exception($"Unknown operator ({op})")
                   };
        }

        private BrunoProgram ParseProgram()
        {
            List<BrunoExpression> statements = new();

            while (!_lexer.IsEnd())
            {
                if (IsNextPunctuation(Linefeed))
                {
                    SkipPunctuation(Linefeed);
                }

                statements.Add(ParseExpression());
            }

            return new BrunoProgram(statements);
        }

        private void SkipPunctuation(char ch)
        {
            if (!IsNextPunctuation(ch)) throw new Exception($"Unexpected punctutation: {_lexer.Next()} {_input.Location}, expected ({ch})");

            _lexer.Next();
        }
    }
}

//[Test]
//public void Parser() {
//    var formulas = new[] {
//        "(1 + 2) * 3",
//        "1 + 2 * 3",
//        "1 + 4 * 2 - 3",
//        "1 + 2 * 3 - a(1, u(2))",
//        "((1 + 2) + 3) + a(1, u(2))",
//        "1 + 2 * 3 - a(1, u(2))",
//        "1 + 1",
//        "1 + 2 * 3 - 4",
//        "1 + 2 * 3 - a(1, u(2))",
//        "1 + Get(1, 2, a(22, z))",
//        "12.3 + add(1)",
//        "regex.StartMatch",
//        "\"yo\".ToString()",
//        "add()",
//        "add().yo() + add(1, 3 + 2).start(x, 7 * 8)",
//        "add().yo(1, 2, 3, 5 * 88)",
//        "add(2, 5).yo(1, 2, 32, 5).Start(x, 1 + 2)",
//        "add(1).yo(1).start(3, 4)",
//        "\"string\".Start",
//        "\"string\".Start(1 + 2, \"ya\").Do(x).No()",
//        "(1 + 2).ToString",
//        "var1",
//        "\"string\"",
//        "a.ToString + 2",
//        "i + j",
//        //"add(i)",
//        //"(i1(2) + 12)",
//        //"add(a(a(i + l) + 2, z, k), j)",
//        //"Len( Match(\"string1\", 14).StartMatch)",
//        //"Value(i1, i2).Start + 1",
//        //"Text(Value(i1, i2) * 0.01, \"0\")",
//        //"Text(Value(i1 + i10 - 3, i2) * 0.01, \"0\").Start + Text(Value(i1, i2).End * 0.01, \"0\") + Text(Value(i1, i2) * 0.01, \"0\") + Text(Value(i1, i2).End2, \"0\")",
//        //@"Concatenate(Text(TimeValue(Mid(Left(i1, Match(i1, ""(?<!\d)([0-2])?\d:[0-6]\d(:[0-6]\d(\.\d+)?)?(\s)*([AaPp][Mm])?(?!\d)"").StartMatch + Len(Match(i1, ""(?<!\d)([0-2])?\d:[0-6]\d(:[0-6]\d(\.\d+)?)?(\s)*([AaPp][Mm])?(?!\d)"").FullMatch) - 1), Match(i1, ""((?<!\d)(\d?\d)(-(\d?\d)-|\/(\d?\d)\/|\.(\d?\d)\.)(19|20)?\d\d(?!\d)|(?<!\d)(19|20)?\d\d(-(\d?\d)-|\/(\d?\d)\/|\.(\d?\d)\.)(\d?\d)(?!\d))"").StartMatch)), ""d mmm yyyy""), Concatenate("" "", Text(DateValue(Mid(Left(i1, (Match(i1, ""(?<!\d)([0-2])?\d:[0-6]\d(:[0-6]\d(\.\d+)?)?(\s)*([AaPp][Mm])?(?!\d)"").StartMatch + Len(Match(i1, ""(?<!\d)([0-2])?\d:[0-6]\d(:[0-6]\d(\.\d+)?)?(\s)*([AaPp][Mm])?(?!\d)"").FullMatch)) - 1), Match(i1, ""((?<!\d)(\d?\d)(-(\d?\d)-|\/(\d?\d)\/|\.(\d?\d)\.)(19|20)?\d\d(?!\d)|(?<!\d)(19|20)?\d\d(-(\d?\d)-|\/(\d?\d)\/|\.(\d?\d)\.)(\d?\d)(?!\d))"").StartMatch), ""en-us""), ""hAM/PM"")))",
//        //"TrimEnds(Mid(Left(i1, (Last(FirstN(MatchAll(i1, \"\\p{Zs}*\\ \\p{Zs}*\"), 3)).StartMatch + Len(Last(FirstN(MatchAll(i1, \"\\p{Zs}*\\ \\p{Zs}*\"), 3)).FullMatch)) - 1), Last(FirstN(MatchAll(i1, \"\\p{Zs}*\\ \\p{Zs}*\"), 2)).StartMatch))"
//    };

//    //var formula =
//    //    @"Text(Round(Value(Mid(Left(i1, (Match(i1, ""[0-9]+(\.[0-9]+)?"").StartMatch + Len(Match(i1, ""[0-9]+(\.[0-9]+)?"").FullMatch)) - 1), Match(i1, ""[-.\p{Lu}\p{Ll}0-9]+"").StartMatch)) / 0.5, 0) * 0.5, ""0"")";

//    var i = 1;
//    BrunoExpression exp = default;
//    //foreach (string formula in _baselineFormulas) {
//    foreach (string formula in formulas) {
//        try {
//            exp = Parser.Parse(formula);
//            Assert.AreEqual(formula.Replace(" ", ""), exp.ToString().Replace(" ", ""));
//            i++;
//            if (formulas.Length < 100) {
//                WriteOutput(formula);
//            }
//        }
//        catch (Exception) {
//            WriteOutput(formula);
//            if (formulas.Length < 100) {
//                throw;
//            }
//        }
//    }

//    void WriteOutput(string formula) {
//        Console.WriteLine($"{i}: ===========================");
//        Console.WriteLine(exp);
//        Console.WriteLine(formula);
//        Console.WriteLine($"--------------------------------");
//        Console.WriteLine("");

//    }
//}