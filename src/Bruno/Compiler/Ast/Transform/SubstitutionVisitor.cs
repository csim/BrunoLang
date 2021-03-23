namespace Bruno.Compiler.Ast.Transform
{
    using System.Collections.Generic;

    /// <summary>
    ///     Visitor that performs variable substitution on BrunoExpressions. Primary usage is to substitute
    ///     variable references with a literal value.
    /// </summary>
    public class SubstitutionVisitor : IVisitor<BrunoExpression>
    {
        private SubstitutionVisitor(BrunoExpression expression,
                                    IReadOnlyDictionary<BrunoExpression, BrunoExpression> substitutions)
        {
            _expression    = expression;
            _substitutions = substitutions;
        }

        private readonly BrunoExpression _expression;
        private readonly IReadOnlyDictionary<BrunoExpression, BrunoExpression> _substitutions;

        public BrunoExpression Visit(BrunoExpression expr)
            => _substitutions.TryGetValue(key: expr, out BrunoExpression result)
                   ? result
                   : this.AcceptChildrenClone(expression: expr);

        /// <summary>
        ///     Perform variable substitutions on the given BrunoExpression.
        /// </summary>
        /// <returns></returns>
        public BrunoExpression Substitute()
            => _expression.Accept(this);

        public static BrunoExpression Substitute(BrunoExpression expression,
                                                 IReadOnlyDictionary<BrunoExpression, BrunoExpression> substitutions)
            => new SubstitutionVisitor(expression: expression, substitutions: substitutions).Substitute();
    }
}