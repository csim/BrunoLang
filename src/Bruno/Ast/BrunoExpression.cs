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

        public IEnumerable<BrunoExpression> Children { get; protected init; }

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

    internal class BrunoFunc : BrunoExpression
    {
        public BrunoFunc(string name, IEnumerable<BrunoExpression> arguments)
        {
            Name = name;
            var args = arguments
                       .TakeWhile(i => i != null)
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
                                out BrunoExpression arg0,
                                out BrunoExpression arg1)
        {
            name = Name;
            arg0 = Arguments.Any() ? Arguments.ElementAt(0) : null;
            arg1 = Arguments.Count() > 1 ? Arguments.ElementAt(1) : null;
        }

        public void Deconstruct(out string name,
                                out BrunoExpression arg0,
                                out BrunoExpression arg1,
                                out BrunoExpression arg2)
        {
            name = Name;
            arg0 = Arguments.Any() ? Arguments.ElementAt(0) : null;
            arg1 = Arguments.Count() > 1 ? Arguments.ElementAt(1) : null;
            arg2 = Arguments.Count() > 2 ? Arguments.ElementAt(2) : null;
        }

        protected override string Serialize()
        {
            if (Arguments == null || !Arguments.Any()) return $"{Name}()";

            var argumentList = string.Join(", ",
                                           Arguments
                                               .TakeWhile(a => a != null)
                                               .Select(a => a.ToString()));

            return $"{Name}({argumentList})";
        }
    }

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

    internal class BrunoNumber : BrunoExpression, IBrunoLiteral
    {
        public BrunoNumber(double value)
        {
            Value    = value;
            Children = new BrunoExpression[0];
        }

        public double Value { get; }

        protected override string Serialize()
            => Value.ToString(CultureInfo.InvariantCulture);
    }

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

    internal class BrunoString : BrunoExpression, IBrunoLiteral
    {
        public BrunoString(string value)
        {
            Value    = value;
            Children = new BrunoExpression[0];
        }

        public string Value { get; }

        protected override string Serialize()
            => EscapeStringLiteral(Value);
    }

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