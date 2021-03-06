namespace Bruno.Compiler.Ast.Transform
{
    using System.Linq;
    using Bruno.Exceptions;
    using static BrunoExpressionHelper;

    /// <summary>
    ///     Visitor pattern for BrunoExpressions
    /// </summary>
    /// <typeparam name="T">Aggregate acceptance type resulting from a visit.</typeparam>
    public interface IVisitor<out T>
    {
        T Visit(BrunoExpression expr);
    }

    public interface IVisitor
    {
        void Visit(BrunoExpression expr);
    }

    public static class VisitorExtensions
    {
        public static void AcceptChildren(this IVisitor visitor, BrunoExpression expression)
        {
            if (expression.Children == null || !expression.Children.Any()) return;

            foreach (BrunoExpression child in expression.Children) child.Accept(visitor);
        }

        public static BrunoExpression AcceptChildrenClone(this IVisitor<BrunoExpression> visitor, BrunoExpression expression)
            => expression switch {
                   BrunoDivide iexpr      => Divide(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoDot iexpr         => Dot(iexpr.Subject.Accept(visitor), iexpr.Accessor),
                   BrunoMinus iexpr       => Minus(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoMultiply iexpr    => Multiply(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoParenthesis iexpr => Parenthesis(iexpr.Body.Accept(visitor)),
                   BrunoPlus iexpr        => Plus(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoAccessor iexpr    => iexpr,
                   BrunoAssign iexpr      => Assign(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoString iexpr      => iexpr,
                   BrunoVariable iexpr    => iexpr,
                   BrunoNumber iexpr      => iexpr,
                   BrunoFunc iexpr        => FuncApp(iexpr.Name, iexpr.Arguments.Select(a => a.Accept(visitor)).ToArray()),
                   _                      => throw new BrunoException($"Invalid BrunoExpression type ({expression.GetType().Name})")
               };
    }
}