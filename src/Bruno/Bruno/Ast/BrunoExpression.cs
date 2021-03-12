namespace Bruno.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using static BrunoExpressionHelper;

    internal interface IBrunoOperator
    {
        BrunoExpression Left { get; }

        int Precedence { get; }

        BrunoExpression Right { get; }
    }

    internal interface IBrunoLiteral
    {
    }

    internal abstract class BrunoExpression : IEquatable<BrunoExpression>
    {
        private string _serialized;

        public IEnumerable<BrunoExpression> Children { get; protected set; }

        public bool Equals(BrunoExpression other)
            => other != null && ToString() == other.ToString();

        public T Accept<T>(IVisitor<T> visitor)
            => visitor.Visit(this);

        public void Accept(IVisitor visitor)
            => visitor.Visit(this);

        public override bool Equals(object other)
            => Equals(other as BrunoExpression);

        public override int GetHashCode()
            => ToString().GetHashCode();

        public static bool operator ==(BrunoExpression left, BrunoExpression right)
            => ReferenceEquals(left, null) && ReferenceEquals(right, null) || left?.Equals(right) == true;

        public static bool operator !=(BrunoExpression left, BrunoExpression right)
            => !(left == right);

        public override string ToString()
            => _serialized ??= Serialize();

        protected abstract string Serialize();
    }

    /// <summary>
    ///     Power Apps generic function
    /// </summary>
    internal class BrunoFunc : BrunoExpression
    {
        public BrunoFunc(string name, IEnumerable<BrunoExpression> arguments)
        {
            Name = name;
            var args = arguments.TakeWhile(i => i != null)
                                .ToArray();
            Arguments = args;
            Children  = args;
        }

        public IEnumerable<BrunoExpression> Arguments { get; }

        public string Name { get; }

        public void Deconstruct(out string name,
                                out BrunoExpression p0)
        {
            name = Name;
            p0   = Arguments.Any() ? Arguments.ElementAt(0) : null;
        }

        public void Deconstruct(out string name,
                                out BrunoExpression p0,
                                out BrunoExpression p1)
        {
            name = Name;
            p0   = Arguments.Any() ? Arguments.ElementAt(0) : null;
            p1   = Arguments.Count() > 1 ? Arguments.ElementAt(1) : null;
        }

        public void Deconstruct(out string name,
                                out BrunoExpression p0,
                                out BrunoExpression p1,
                                out BrunoExpression p2)
        {
            name = Name;
            p0   = Arguments.Any() ? Arguments.ElementAt(0) : null;
            p1   = Arguments.Count() > 1 ? Arguments.ElementAt(1) : null;
            p2   = Arguments.Count() > 2 ? Arguments.ElementAt(2) : null;
        }

        protected override string Serialize()
        {
            if (Arguments == null || !Arguments.Any()) return $"{Name}()";

            // Some Power Apps functions accept optional arguments, such as Mid()
            // when an argument is null and located at the end of the array, it is ignored
            var argumentList = string.Join(", ",
                                           Arguments.TakeWhile(a => a != null)
                                                    .Select(a => a.ToString()));

            return $"{Name}({argumentList})";
        }
    }

    /// <summary>
    ///     Power Apps dot operator
    /// </summary>
    internal class BrunoDot : BrunoExpression
    {
        public BrunoDot(BrunoExpression subject, BrunoExpression accessor)
        {
            Subject  = subject;
            Accessor = accessor;
            Children = new[] { subject };
        }

        public BrunoExpression Accessor { get; }

        public BrunoExpression Subject { get; }

        protected override string Serialize()
            => $"{Subject}.{Accessor}";
    }

    /// <summary>
    ///     f
    ///     Power Apps parenthesis grouping
    /// </summary>
    internal class BrunoParenthesis : BrunoExpression
    {
        public BrunoParenthesis(BrunoExpression body)
        {
            Body     = body;
            Children = new[] { body };
        }

        public BrunoExpression Body { get; }

        protected override string Serialize()
            => $"({Body})";
    }

    /// <summary>
    ///     Power Apps integer
    /// </summary>
    internal class BrunoIntLiteral : BrunoExpression, IBrunoLiteral
    {
        public BrunoIntLiteral(int value)
        {
            Value    = value;
            Children = new BrunoExpression[0];
        }

        public int Value { get; }

        protected override string Serialize()
            => Value.ToString();
    }

    /// <summary>
    ///     Power Apps double
    /// </summary>
    internal class BrunoDoubleLiteral : BrunoExpression, IBrunoLiteral
    {
        public BrunoDoubleLiteral(double value)
        {
            Value    = value;
            Children = new BrunoExpression[0];
        }

        public double Value { get; }

        protected override string Serialize()
            => Value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Power Apps minus operator
    /// </summary>
    internal class BrunoMinus : BrunoExpression, IBrunoOperator
    {
        public BrunoMinus(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[]
                       {
                           left,
                           right
                       };
        }

        public BrunoExpression Left { get; }

        public BrunoExpression Right { get; }

        public int Precedence => 4;

        protected override string Serialize()
            => $"{Left} - {Right}";
    }

    /// <summary>
    ///     Power Apps plus operator
    /// </summary>
    internal class BrunoPlus : BrunoExpression, IBrunoOperator
    {
        public BrunoPlus(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[]
                       {
                           left,
                           right
                       };
        }

        public BrunoExpression Left { get; }

        public BrunoExpression Right { get; }

        public int Precedence => 4;

        protected override string Serialize()
            => $"{Left} + {Right}";
    }

    internal class BrunoMultiply : BrunoExpression, IBrunoOperator
    {
        public BrunoMultiply(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[]
                       {
                           left,
                           right
                       };
        }

        public BrunoExpression Left { get; }

        public BrunoExpression Right { get; }

        public int Precedence => 2;

        protected override string Serialize()
            => $"{Left} * {Right}";
    }

    internal class BrunoDivide : BrunoExpression, IBrunoOperator
    {
        public BrunoDivide(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[]
                       {
                           left,
                           right
                       };
        }

        public BrunoExpression Left { get; }

        public BrunoExpression Right { get; }

        public int Precedence => 2;

        protected override string Serialize()
            => $"{Left} / {Right}";
    }

    /// <summary>
    ///     Power Apps string
    /// </summary>
    internal class BrunoStringLiteral : BrunoExpression, IBrunoLiteral
    {
        public BrunoStringLiteral(string value)
        {
            Value    = value;
            Children = new BrunoExpression[0];
        }

        public string Value { get; }

        protected override string Serialize()
            => EscapeStringLiteral(Value);
    }

    /// <summary>
    ///     Power Apps variable
    /// </summary>
    internal class BrunoVariable : BrunoExpression
    {
        public BrunoVariable(string name)
        {
            Name     = name;
            Children = new BrunoExpression[0];
        }

        public string Name { get; }

        protected override string Serialize()
            => Name;
    }

    /// <summary>
    ///     Power Apps With2 construct.
    ///     https://docs.microsoft.com/en-us/Brunos/maker/canvas-apps/functions/function-with
    /// </summary>
    internal class BrunoWith : BrunoExpression
    {
        public BrunoWith(IReadOnlyDictionary<string, BrunoExpression> context, BrunoExpression body)
        {
            Context = context;
            Body    = body;
            var children = context.Values.ToList();
            children.Add(body);
            Children = children.ToArray();
        }

        public BrunoExpression Body { get; }

        public IReadOnlyDictionary<string, BrunoExpression> Context { get; }

        protected override string Serialize()
            => $"With({{ {string.Join(", ", Context.Select(item => $"{item.Key}: {item.Value}"))} }}, {Body})";
    }

    /// <summary>
    ///     Power Apps property accessor
    /// </summary>
    internal class BrunoAccessor : BrunoExpression
    {
        public BrunoAccessor(string name)
        {
            Name     = name;
            Children = new BrunoExpression[0];
        }

        public string Name { get; }

        protected override string Serialize()
            => Name;
    }
}