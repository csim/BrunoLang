﻿namespace Bruno.Ast
{
    using System.Linq;
    using Bruno.Exceptions;
    using static BrunoExpressionHelper;

    /// <summary>
    ///     Visitor pattern for BrunoExpressions
    /// </summary>
    /// <typeparam name="T">Aggregate acceptance type resulting from a visit.</typeparam>
    internal interface IVisitor<out T>
    {
        T Visit(BrunoExpression expr);
    }

    internal interface IVisitor
    {
        void Visit(BrunoExpression expr);
    }

    internal static class VisitorExtensions
    {
        public static void AcceptChildren(this IVisitor visitor, BrunoExpression expression)
        {
            if (expression.Children == null || !expression.Children.Any()) return;

            foreach (var child in expression.Children) child.Accept(visitor);
        }

        public static BrunoExpression AcceptChildrenClone(this IVisitor<BrunoExpression> visitor, BrunoExpression expression)
            => expression switch
               {
                   BrunoAccessor iexpr      => iexpr,
                   BrunoDivide iexpr        => Divide(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoDot iexpr           => Dot(iexpr.Subject.Accept(visitor), iexpr.Accessor),
                   BrunoDoubleLiteral iexpr => iexpr,
                   BrunoFunc iexpr          => FuncApp(iexpr.Name,
                                                       iexpr.Arguments
                                                            .TakeWhile(a => a != null)
                                                            .Select(a => a.Accept(visitor))
                                                            .ToArray()),
                   BrunoIntLiteral iexpr    => iexpr,
                   BrunoMinus iexpr         => Minus(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoMultiply iexpr      => Multiply(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoParenthesis iexpr   => Parenthesis(iexpr.Body.Accept(visitor)),
                   BrunoPlus iexpr          => Plus(iexpr.Left.Accept(visitor), iexpr.Right.Accept(visitor)),
                   BrunoStringLiteral iexpr => iexpr,
                   BrunoVariable iexpr      => iexpr,
                   BrunoWith iexpr          => With(iexpr.Context, iexpr.Body.Accept(visitor)),
                   _                        => throw new BrunoException($"Invalid BrunoExpression type ({expression.GetType().Name})")
               };
    }
}