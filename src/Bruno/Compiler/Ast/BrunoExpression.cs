namespace Bruno.Compiler.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Bruno.Compiler.Ast.Transform;
    using Bruno.Interpreter;
    using static BrunoExpressionHelper;

    public interface IBrunoOperator
    {
        BrunoExpression Left { get; }

        int Precedence { get; }

        BrunoExpression Right { get; }
    }

    public interface IBrunoLiteral
    {
    }

    public abstract class BrunoExpression : IEquatable<BrunoExpression>
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

        public object Evaluate()
            => InterpreterService.Evaluate(this);

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

    public class BrunoProgram : BrunoExpression
    {
        public BrunoProgram(IEnumerable<BrunoExpression> statements)
        {
            Statements = statements;
            Children   = statements;
        }

        public IEnumerable<BrunoExpression> Statements { get; }

        protected override string Serialize()
        {
            StringBuilder content = new();

            foreach (BrunoExpression statement in Statements)
            {
                content.AppendLine(statement.ToString());
            }

            return content.ToString();
        }
    }

    public class BrunoFunc : BrunoExpression
    {
        public BrunoFunc(string name, IEnumerable<BrunoExpression> arguments)
        {
            Name = name;
            BrunoExpression[] args = arguments
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

            string argumentList = string.Join(", ",
                                              Arguments
                                                  .TakeWhile(a => a != null)
                                                  .Select(a => a.ToString()));

            return $"{Name}({argumentList})";
        }
    }

    public class BrunoDot : BrunoExpression
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

    public class BrunoParenthesis : BrunoExpression
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

    public class BrunoNumber : BrunoExpression, IBrunoLiteral
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

    public class BrunoMinus : BrunoExpression, IBrunoOperator
    {
        public BrunoMinus(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[] {
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

    public class BrunoPlus : BrunoExpression, IBrunoOperator
    {
        public BrunoPlus(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[] {
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

    public class BrunoAssign : BrunoExpression, IBrunoOperator
    {
        public BrunoAssign(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[] {
                                 left,
                                 right
                             };
        }

        public BrunoExpression Left { get; }

        public BrunoExpression Right { get; }

        public int Precedence => 1;

        protected override string Serialize()
            => $"{Left} = {Right}";
    }

    public class BrunoMultiply : BrunoExpression, IBrunoOperator
    {
        public BrunoMultiply(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[] {
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

    public class BrunoDivide : BrunoExpression, IBrunoOperator
    {
        public BrunoDivide(BrunoExpression left, BrunoExpression right)
        {
            if (left is IBrunoOperator ileft && ileft.Precedence < Precedence) left = Parenthesis(left);

            if (right is IBrunoOperator iright && iright.Precedence < Precedence) right = Parenthesis(right);

            Left  = left;
            Right = right;
            Children = new[] {
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

    public class BrunoString : BrunoExpression, IBrunoLiteral
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

    public class BrunoVariable : BrunoExpression
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

    public class BrunoAccessor : BrunoExpression
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