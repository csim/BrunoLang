﻿namespace Bruno.Interpreter
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
                   BrunoProgram isubject     => VisitProgram(program: isubject),
                   BrunoAssign isubject      => VisitAssign(assign: isubject),
                   BrunoAccessor isubject    => VisitAccessor(subject: isubject),
                   BrunoDivide isubject      => VisitDivide(subject: isubject),
                   BrunoDot isubject         => VisitDot(subject: isubject),
                   BrunoNumber isubject      => VisitNumber(subject: isubject),
                   BrunoFunc isubject        => VisitFunc(subject: isubject),
                   BrunoMinus isubject       => VisitMinus(subject: isubject),
                   BrunoMultiply isubject    => VisitMultiply(subject: isubject),
                   BrunoParenthesis isubject => VisitParenthesis(subject: isubject),
                   BrunoPlus isubject        => VisitPlus(subject: isubject),
                   BrunoString isubject      => VisitString(subject: isubject),
                   BrunoVariable isubject    => VisitVariable(subject: isubject),
                   var _                     => throw new ArgumentOutOfRangeException(nameof(subject))
               };

        public static object Evaluate(BrunoExpression expression)
            => new InterpreterService().Accept(subject: expression);

        private object Accept(BrunoExpression subject)
            => subject?.Accept(this);

        private static T ChangeType<T>(object value)
            => (T)Convert.ChangeType(value: value, typeof(T));

        private static Dictionary<string, object> ToDictionary(Match match)
            => new() {
                         ["FullMatch"]  = match.Value,
                         ["StartMatch"] = match.Index + 1
                     };

        private static object VisitAccessor(BrunoAccessor subject)
            => subject.Name;

        private object VisitAssign(BrunoAssign assign)
            => _variableLookup[key: ((BrunoVariable)assign.Left).Name] = Accept(subject: assign.Right);

        private object VisitConcatenate(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression leftExp, BrunoExpression rightExp) = subject;

            return (string)Accept(subject: leftExp) + (string)Accept(subject: rightExp);
        }

        private object VisitDateTimeValue(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression localeExp) = subject;

            string inputVal = (string)Accept(subject: inputExp);
            string localeVal = localeExp != null
                                   ? (string)Accept(subject: localeExp)
                                   : "en-us";

            DateTimeFormatInfo formatProvider = new CultureInfo(name: localeVal).DateTimeFormat;

            return DateTime.Parse(s: inputVal, provider: formatProvider);
        }

        private object VisitDivide(BrunoDivide subject)
            => ChangeType<double>(ChangeType<double>(Accept(subject: subject.Left))) / ChangeType<double>(Accept(subject: subject.Right));

        private object VisitDot(BrunoDot subject)
        {
            Dictionary<string, object> leftVal = (Dictionary<string, object>)Accept(subject: subject.Subject);

            return leftVal[subject.Accessor.ToString()];
        }

        private object VisitFind(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression findExp) = subject;
            string inputVal = (string)Accept(subject: inputExp);
            string findVal  = (string)Accept(subject: findExp);

            int index = findVal.IndexOf(value: inputVal, comparisonType: StringComparison.Ordinal);

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
            object[] listVal  = (object[])Accept(subject: listExp);
            int      countVal = ChangeType<int>(Accept(subject: countExp));

            return listVal.Take(count: countVal).ToArray();
        }

        private object VisitFunc(BrunoFunc subject)
            => subject.Name switch {
                   nameof(BrunoExpressionHelper.Concatenate)   => VisitConcatenate(subject: subject),
                   nameof(BrunoExpressionHelper.Left)          => VisitLeft(subject: subject),
                   nameof(BrunoExpressionHelper.Right)         => VisitRight(subject: subject),
                   nameof(BrunoExpressionHelper.Mid)           => VisitMid(subject: subject),
                   nameof(BrunoExpressionHelper.Len)           => VisitLen(subject: subject),
                   nameof(BrunoExpressionHelper.Proper)        => VisitProper(subject: subject),
                   nameof(BrunoExpressionHelper.Lower)         => VisitLower(subject: subject),
                   nameof(BrunoExpressionHelper.Upper)         => VisitUpper(subject: subject),
                   nameof(BrunoExpressionHelper.Match)         => VisitMatch(subject: subject),
                   nameof(BrunoExpressionHelper.MatchAll)      => VisitMatchAll(subject: subject),
                   nameof(BrunoExpressionHelper.Split)         => VisitSplit(subject: subject),
                   nameof(BrunoExpressionHelper.Find)          => VisitFind(subject: subject),
                   nameof(BrunoExpressionHelper.First)         => VisitFirst(subject: subject),
                   nameof(BrunoExpressionHelper.FirstN)        => VisitFirstN(subject: subject),
                   nameof(BrunoExpressionHelper.Last)          => VisitLast(subject: subject),
                   nameof(BrunoExpressionHelper.LastN)         => VisitLastN(subject: subject),
                   nameof(BrunoExpressionHelper.TrimEnds)      => VisitTrimEnds(subject: subject),
                   nameof(BrunoExpressionHelper.Text)          => VisitText(subject: subject),
                   nameof(BrunoExpressionHelper.Value)         => VisitValue(subject: subject),
                   nameof(BrunoExpressionHelper.Round)         => VisitRound(subject: subject),
                   nameof(BrunoExpressionHelper.RoundUp)       => VisitRoundUp(subject: subject),
                   nameof(BrunoExpressionHelper.RoundDown)     => VisitRoundDown(subject: subject),
                   nameof(BrunoExpressionHelper.DateTimeValue) => VisitDateTimeValue(subject: subject),
                   "print"                                     => VisitPrint(subject: subject),
                   var _                                       => throw new BrunoRuntimeException($"{subject.Name} not implemented.")
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
            object[] inputVals = (object[])Accept(subject: inputExp);
            int      indexVal  = ChangeType<int>(Accept(subject: indexExp));

            return inputVals.TakeLast(count: indexVal).ToArray();
        }

        private object VisitLeft(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression lengthExp) = subject;
            string inputVal  = (string)Accept(subject: inputExp);
            int    lengthVal = ChangeType<int>(Accept(subject: lengthExp));

            if (lengthVal <= 0)
            {
                throw new BrunoRuntimeException($"Invalid length parameter ({lengthVal}) for {subject.Name}()");
            }

            return inputVal.Substring(0, length: lengthVal);
        }

        private object VisitLen(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp) = subject;
            string inputVal = (string)Accept(subject: inputExp);

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
            string inputVal = (string)Accept(subject: inputExp);
            string regexVal = (string)Accept(subject: regexExp);

            Match match = Regex.Match(input: inputVal, pattern: regexVal);
            return match.Success ? ToDictionary(match: match) : null;
        }

        private object VisitMatchAll(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression regexExp) = subject;
            string inputVal = (string)Accept(subject: inputExp);
            string regexVal = (string)Accept(subject: regexExp);

            MatchCollection matches = Regex.Matches(input: inputVal, pattern: regexVal);
            return matches.Select(selector: ToDictionary).ToArray();
        }

        private object VisitMid(BrunoFunc subject)
        {
            if (subject.Arguments.Count() < 2 || subject.Arguments.Count() > 3)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression startIndexExp, BrunoExpression lengthExp) = subject;
            string inputVal      = (string)Accept(subject: inputExp);
            int    startIndexVal = ChangeType<int>(Accept(subject: startIndexExp));
            int?   lengthVal     = lengthExp == null ? (int?)null : ChangeType<int>(Accept(subject: lengthExp));

            if (startIndexVal < 1)
            {
                return string.Empty;
            }

            return lengthVal.HasValue
                       ? inputVal.Substring(startIndexVal - 1, length: lengthVal.Value)
                       : inputVal.Substring(startIndexVal - 1);
        }

        private object VisitMinus(BrunoMinus subject)
            => ChangeType<double>(Accept(subject: subject.Left)) - ChangeType<double>(Accept(subject: subject.Right));

        private object VisitMultiply(BrunoMultiply subject)
            => ChangeType<double>(ChangeType<double>(Accept(subject: subject.Left))) * ChangeType<double>(Accept(subject: subject.Right));

        private static object VisitNumber(BrunoNumber subject)
            => subject.Value;

        private object VisitParenthesis(BrunoParenthesis subject)
            => Accept(subject: subject.Body);

        private object VisitPlus(BrunoPlus subject)
            => ChangeType<double>(Accept(subject: subject.Left)) + ChangeType<double>(Accept(subject: subject.Right));

        private object VisitPrint(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            object raw = Accept(subject.Arguments.ElementAt(0));

            Console.WriteLine(value: raw);
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
            string inputVal  = (string)Accept(subject: inputExp);
            int    lengthVal = ChangeType<int>(Accept(subject: lengthExp));

            if (lengthVal <= 0) throw new BrunoRuntimeException($"Invalid length parameter ({lengthVal}) for {subject.Name}()");

            return inputVal.Substring(inputVal.Length - lengthVal);
        }

        private object VisitRound(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression decimalsExp) = subject;

            decimal inputVal    = Convert.ToDecimal(Accept(subject: inputExp));
            int     decimalsVal = ChangeType<int>(Accept(subject: decimalsExp));

            return Math.Round(d: inputVal, decimals: decimalsVal, mode: MidpointRounding.AwayFromZero);
        }

        private object VisitRoundDown(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression decimalsExp) = subject;
            double inputVal    = Convert.ToDouble(Accept(subject: inputExp));
            int    decimalsVal = ChangeType<int>(Accept(subject: decimalsExp));

            double multiplier = Math.Pow(10, Convert.ToDouble(value: decimalsVal));
            return Math.Floor(inputVal * multiplier) / multiplier;
        }

        private object VisitRoundUp(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression decimalsExp) = subject;
            double inputVal    = Convert.ToDouble(Accept(subject: inputExp));
            int    decimalsVal = ChangeType<int>(Accept(subject: decimalsExp));

            double multiplier = Math.Pow(10, Convert.ToDouble(value: decimalsVal));
            return Math.Ceiling(inputVal * multiplier) / multiplier;
        }

        private object VisitSplit(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 2)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp, BrunoExpression delimiterExp) = subject;
            string inputVal     = (string)Accept(subject: inputExp);
            string delimiterVal = (string)Accept(subject: delimiterExp);

            if (inputVal == null) throw new BrunoRuntimeException($"Input string cannot be null {subject.Name}()");

            return inputVal.Split(new[] { delimiterVal }, options: StringSplitOptions.None);
        }

        private static object VisitString(BrunoString subject)
            => subject.Value;

        private object VisitText(BrunoFunc subject)
        {
            (string _, BrunoExpression inputExp, BrunoExpression formatExp) = subject;
            object inputVal  = Accept(subject: inputExp);
            string formatVal = (string)Accept(subject: formatExp);

            object VisitTextDate(DateTime dateTimeVal, string formatString)
            {
                formatString = Regex.Replace(input: formatString,
                                             @"(?<!\:|m)m*",
                                             match => match.Value.ToUpper());

                formatString = formatString.Replace("AM/PM", "tt");

                return dateTimeVal.ToString(format: formatString);
            }

            return inputVal switch {
                       int int32Val       => int32Val.ToString(format: formatVal),
                       double doubleVal   => doubleVal.ToString(format: formatVal),
                       decimal decimalVal => decimalVal.ToString(format: formatVal),
                       DateTime dateVal   => VisitTextDate(dateTimeVal: dateVal, formatString: formatVal),
                       var _              => throw new BrunoRuntimeException($"Invalid Text() input: {inputVal} ({inputVal.GetType().Name})")
                   };
        }

        private object VisitTrimEnds(BrunoFunc subject)
        {
            if (subject.Arguments.Count() != 1)
            {
                throw new BrunoRuntimeException($"Invalid number of arguments ({subject.Arguments.Count()}) for {subject.Name}()");
            }

            (string _, BrunoExpression inputExp) = subject;
            string inputVal = (string)Accept(subject: inputExp);

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
            string             inputVal     = (string)Accept(subject: inputExp);
            const NumberStyles numberStyles = NumberStyles.Any | NumberStyles.AllowExponent;

            if (int.TryParse(s: inputVal, style: numberStyles, provider: CultureInfo.InvariantCulture, out int intVal))
            {
                return intVal;
            }

            if (decimal.TryParse(s: inputVal, style: numberStyles, provider: CultureInfo.InvariantCulture, out decimal decimalVal))
            {
                return decimalVal;
            }

            throw new BrunoRuntimeException($"Invalid Value() input: {inputVal} ({inputVal?.GetType().Name})");
        }

        private object VisitVariable(BrunoVariable subject)
        {
            if (!_variableLookup.TryGetValue(key: subject.Name, out object value))
            {
                throw new ApplicationException($"Variable not found ({subject.Name})");
            }

            return value;
        }
    }
}