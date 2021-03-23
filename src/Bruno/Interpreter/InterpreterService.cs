namespace Bruno.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Bruno.Compiler.Ast;
    using Bruno.Compiler.Ast.Transform;
    using Bruno.Exceptions;

    public class InterpreterService : IVisitor<object>
    {
        private InterpreterService()
        {
            _variableLookup = new Dictionary<string, object>();
        }

        private readonly IDictionary<string, object> _variableLookup;

        public object Visit(BrunoExpression subject)
            => subject switch {
                   BrunoProgram isubject     => VisitProgram(isubject),
                   BrunoAssign isubject      => VisitAssign(isubject),
                   BrunoAccessor isubject    => VisitAccessor(isubject),
                   BrunoDivide isubject      => VisitDivide(isubject),
                   BrunoDot isubject         => VisitDot(isubject),
                   BrunoNumber isubject      => VisitNumber(isubject),
                   BrunoFunc isubject        => VisitFunc(isubject),
                   BrunoMinus isubject       => VisitMinus(isubject),
                   BrunoMultiply isubject    => VisitMultiply(isubject),
                   BrunoParenthesis isubject => VisitParenthesis(isubject),
                   BrunoPlus isubject        => VisitPlus(isubject),
                   BrunoString isubject      => VisitString(isubject),
                   BrunoVariable isubject    => VisitVariable(isubject),
                   _                         => throw new ArgumentOutOfRangeException(nameof(subject))
               };

        public static object Evaluate(BrunoExpression expression)
            => new InterpreterService().Accept(expression);

        private object Accept(BrunoExpression subject)
            => subject?.Accept(this);

        private static T ChangeType<T>(object value)
            => (T)Convert.ChangeType(value, typeof(T));

        private static Dictionary<string, object> ToDictionary(Match match)
            => new() {
                         ["FullMatch"]  = match.Value,
                         ["StartMatch"] = match.Index + 1
                     };

        private static object VisitAccessor(BrunoAccessor subject)
            => subject.Name;

        private object VisitAssign(BrunoAssign assign)
            => _variableLookup[((BrunoVariable)assign.Left).Name] = Accept(assign.Right);

        private object VisitConcatenate(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression leftExp, BrunoExpression rightExp) = subject;

            return (string)Accept(leftExp) + (string)Accept(rightExp);
        }

        private object VisitDateTimeValue(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression localeExp) = subject;

            string inputVal = (string)Accept(inputExp);
            string localeVal = localeExp != null
                                   ? (string)Accept(localeExp)
                                   : "en-us";

            DateTimeFormatInfo formatProvider = new CultureInfo(localeVal).DateTimeFormat;

            return DateTime.Parse(inputVal, formatProvider);
        }

        private object VisitDivide(BrunoDivide subject)
            => ChangeType<double>(ChangeType<double>(Accept(subject.Left))) / ChangeType<double>(Accept(subject.Right));

        private object VisitDot(BrunoDot subject)
        {
            Dictionary<string, object> leftVal = (Dictionary<string, object>)Accept(subject.Subject);

            return leftVal[subject.Accessor.ToString()];
        }

        private object VisitFind(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression findExp) = subject;
            string inputVal = (string)Accept(inputExp);
            string findVal  = (string)Accept(findExp);

            int index = findVal.IndexOf(inputVal, StringComparison.Ordinal);

            return index != -1 ? index + 1 : (int?)null;
        }

        private object VisitFirst(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments (subject.Arguments.Length) for {subject.Name}()");
            }

            object[] listVal = (object[])Accept(subject.Arguments.ElementAt(0));
            return listVal.FirstOrDefault();
        }

        private object VisitFirstN(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression listExp, BrunoExpression countExp) = subject;
            object[] listVal  = (object[])Accept(listExp);
            int      countVal = ChangeType<int>(Accept(countExp));

            return listVal.Take(countVal).ToArray();
        }

        private object VisitFunc(BrunoFunc subject)
            => subject.Name switch {
                   nameof(BrunoExpressionHelper.Concatenate)   => VisitConcatenate(subject),
                   nameof(BrunoExpressionHelper.Left)          => VisitLeft(subject),
                   nameof(BrunoExpressionHelper.Right)         => VisitRight(subject),
                   nameof(BrunoExpressionHelper.Mid)           => VisitMid(subject),
                   nameof(BrunoExpressionHelper.Len)           => VisitLen(subject),
                   nameof(BrunoExpressionHelper.Proper)        => VisitProper(subject),
                   nameof(BrunoExpressionHelper.Lower)         => VisitLower(subject),
                   nameof(BrunoExpressionHelper.Upper)         => VisitUpper(subject),
                   nameof(BrunoExpressionHelper.Match)         => VisitMatch(subject),
                   nameof(BrunoExpressionHelper.MatchAll)      => VisitMatchAll(subject),
                   nameof(BrunoExpressionHelper.Split)         => VisitSplit(subject),
                   nameof(BrunoExpressionHelper.Find)          => VisitFind(subject),
                   nameof(BrunoExpressionHelper.First)         => VisitFirst(subject),
                   nameof(BrunoExpressionHelper.FirstN)        => VisitFirstN(subject),
                   nameof(BrunoExpressionHelper.Last)          => VisitLast(subject),
                   nameof(BrunoExpressionHelper.LastN)         => VisitLastN(subject),
                   nameof(BrunoExpressionHelper.TrimEnds)      => VisitTrimEnds(subject),
                   nameof(BrunoExpressionHelper.Text)          => VisitText(subject),
                   nameof(BrunoExpressionHelper.Value)         => VisitValue(subject),
                   nameof(BrunoExpressionHelper.Round)         => VisitRound(subject),
                   nameof(BrunoExpressionHelper.RoundUp)       => VisitRoundUp(subject),
                   nameof(BrunoExpressionHelper.RoundDown)     => VisitRoundDown(subject),
                   nameof(BrunoExpressionHelper.DateTimeValue) => VisitDateTimeValue(subject),
                   "print"                                     => VisitPrint(subject),
                   _                                           => throw new BrunoRuntimeException($"{subject.Name} not implemented.")
               };

        private object VisitLast(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            object[] listVal = (object[])Accept(subject.Arguments.ElementAt(0));
            return listVal.LastOrDefault();
        }

        private object VisitLastN(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression indexExp) = subject;
            object[] inputVals = (object[])Accept(inputExp);
            int      indexVal  = ChangeType<int>(Accept(indexExp));

            return inputVals.TakeLast(indexVal).ToArray();
        }

        private object VisitLeft(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression lengthExp) = subject;
            string inputVal  = (string)Accept(inputExp);
            int    lengthVal = ChangeType<int>(Accept(lengthExp));

            if (lengthVal <= 0)
            {
                throw new BrunoRuntimeException($"Invalid length parameter ({lengthVal}) for {subject.Name}()");
            }

            return inputVal.Substring(0, lengthVal);
        }

        private object VisitLen(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp) = subject;
            string inputVal = (string)Accept(inputExp);

            return inputVal?.Length;
        }

        private object VisitLower(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            return ((string)Accept(subject.Arguments.ElementAt(0))).ToLowerInvariant();
        }

        private object VisitMatch(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression regexExp) = subject;
            string inputVal = (string)Accept(inputExp);
            string regexVal = (string)Accept(regexExp);

            Match match = Regex.Match(inputVal, regexVal);
            return match.Success ? ToDictionary(match) : null;
        }

        private object VisitMatchAll(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression regexExp) = subject;
            string inputVal = (string)Accept(inputExp);
            string regexVal = (string)Accept(regexExp);

            MatchCollection matches = Regex.Matches(inputVal, regexVal);
            return matches.Select(ToDictionary).ToArray();
        }

        private object VisitMid(BrunoFunc subject)
        {
            if (subject.Arguments.Count() < 2 || subject.Arguments.Count() > 3)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression startIndexExp, BrunoExpression lengthExp) = subject;
            string inputVal      = (string)Accept(inputExp);
            int    startIndexVal = ChangeType<int>(Accept(startIndexExp));
            int?   lengthVal     = lengthExp == null ? (int?)null : ChangeType<int>(Accept(lengthExp));

            if (startIndexVal < 1)
            {
                return string.Empty;
            }

            return lengthVal.HasValue
                       ? inputVal.Substring(startIndexVal - 1, lengthVal.Value)
                       : inputVal.Substring(startIndexVal - 1);
        }

        private object VisitMinus(BrunoMinus subject)
            => ChangeType<double>(Accept(subject.Left)) - ChangeType<double>(Accept(subject.Right));

        private object VisitMultiply(BrunoMultiply subject)
            => ChangeType<double>(ChangeType<double>(Accept(subject.Left))) * ChangeType<double>(Accept(subject.Right));

        private static object VisitNumber(BrunoNumber subject)
            => subject.Value;

        private object VisitParenthesis(BrunoParenthesis subject)
            => Accept(subject.Body);

        private object VisitPlus(BrunoPlus subject)
            => ChangeType<double>(Accept(subject.Left)) + ChangeType<double>(Accept(subject.Right));

        private object VisitPrint(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            object raw = Accept(subject.Arguments.ElementAt(0));

            Console.WriteLine(raw);
            return null;
        }

        private object VisitProgram(BrunoProgram program)
        {
            foreach (BrunoExpression statement in program.Statements)
            {
                statement.Accept(this);
            }

            return null;
        }

        private object VisitProper(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            TextInfo ti       = CultureInfo.InvariantCulture.TextInfo;
            string   inputVal = (string)Accept(subject.Arguments.ElementAt(0));

            return ti.ToTitleCase(inputVal.ToLowerInvariant());
        }

        private object VisitRight(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression lengthExp) = subject;
            string inputVal  = (string)Accept(inputExp);
            int    lengthVal = ChangeType<int>(Accept(lengthExp));

            if (lengthVal <= 0) throw new BrunoRuntimeException($"Invalid length parameter ({lengthVal}) for {subject.Name}()");

            return inputVal.Substring(inputVal.Length - lengthVal);
        }

        private object VisitRound(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression decimalsExp) = subject;

            decimal inputVal    = Convert.ToDecimal(Accept(inputExp));
            int     decimalsVal = ChangeType<int>(Accept(decimalsExp));

            return Math.Round(inputVal, decimalsVal, MidpointRounding.AwayFromZero);
        }

        private object VisitRoundDown(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression decimalsExp) = subject;
            double inputVal    = Convert.ToDouble(Accept(inputExp));
            int    decimalsVal = ChangeType<int>(Accept(decimalsExp));

            double multiplier = Math.Pow(10, Convert.ToDouble(decimalsVal));
            return Math.Floor(inputVal * multiplier) / multiplier;
        }

        private object VisitRoundUp(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression decimalsExp) = subject;
            double inputVal    = Convert.ToDouble(Accept(inputExp));
            int    decimalsVal = ChangeType<int>(Accept(decimalsExp));

            double multiplier = Math.Pow(10, Convert.ToDouble(decimalsVal));
            return Math.Ceiling(inputVal * multiplier) / multiplier;
        }

        private object VisitSplit(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression delimiterExp) = subject;
            string inputVal     = (string)Accept(inputExp);
            string delimiterVal = (string)Accept(delimiterExp);

            if (inputVal == null) throw new BrunoRuntimeException($"Input string cannot be null {subject.Name}()");

            return inputVal.Split(new[] { delimiterVal }, StringSplitOptions.None);
        }

        private static object VisitString(BrunoString subject)
            => subject.Value;

        private object VisitText(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression formatExp) = subject;
            object inputVal  = Accept(inputExp);
            string formatVal = (string)Accept(formatExp);

            object VisitTextDate(DateTime dateTimeVal, string formatString)
            {
                formatString = Regex.Replace(formatString,
                                             @"(?<!\:|m)m*",
                                             match => match.Value.ToUpper());

                formatString = formatString.Replace("AM/PM", "tt");

                return dateTimeVal.ToString(formatString);
            }

            return inputVal switch {
                       int int32Val       => int32Val.ToString(formatVal),
                       double doubleVal   => doubleVal.ToString(formatVal),
                       decimal decimalVal => decimalVal.ToString(formatVal),
                       DateTime dateVal   => VisitTextDate(dateVal, formatVal),
                       _                  => throw new BrunoRuntimeException($"Invalid Text() input: {inputVal} ({inputVal.GetType().Name})")
                   };
        }

        private object VisitTrimEnds(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp) = subject;
            string inputVal = (string)Accept(inputExp);

            return inputVal.Trim();
        }

        private object VisitUpper(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            return ((string)Accept(subject.Arguments.ElementAt(0))).ToUpperInvariant();
        }

        private object VisitValue(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp) = subject;
            string             inputVal     = (string)Accept(inputExp);
            const NumberStyles numberStyles = NumberStyles.Any | NumberStyles.AllowExponent;

            if (int.TryParse(inputVal, numberStyles, CultureInfo.InvariantCulture, out int intVal))
            {
                return intVal;
            }

            if (decimal.TryParse(inputVal, numberStyles, CultureInfo.InvariantCulture, out decimal decimalVal))
            {
                return decimalVal;
            }

            throw new BrunoRuntimeException($"Invalid Value() input: {inputVal} ({inputVal?.GetType().Name})");
        }

        private object VisitVariable(BrunoVariable subject)
        {
            if (!_variableLookup.TryGetValue(subject.Name, out object value))
            {
                throw new ApplicationException($"Variable not found ({subject.Name})");
            }

            return value;
        }
    }
}