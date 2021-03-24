namespace Bruno.Compiler.Ast.Transform
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Visitor that extracts a list of expressions that match a given predicate.
    /// </summary>
    public class ExtractVisitor : IVisitor
    {
        private ExtractVisitor([NotNull] BrunoExpression expression, [NotNull] Func<BrunoExpression, bool> extractAction)
        {
            _expression = expression    ?? throw new ArgumentNullException(nameof(expression));
            _predicate  = extractAction ?? throw new ArgumentNullException(nameof(extractAction));
        }

        private          int                         _depth = 1;
        private readonly BrunoExpression             _expression;
        private          BrunoExpression             _parent;
        private readonly Func<BrunoExpression, bool> _predicate;
        private readonly IList<BrunoExtractResult>   _results = new List<BrunoExtractResult>();

        public void Visit(BrunoExpression node)
        {
            if (_predicate(node)) _results.Add(new BrunoExtractResult(_parent, _depth, node));

            if (node.Children == null || !node.Children.Any()) return;

            _parent = node;
            _depth++;
            this.AcceptChildren(node);
            _depth--;
            _parent = null;
        }

        public IEnumerable<BrunoExtractResult> Extract()
        {
            _expression.Accept(this);

            return _results;
        }

        public static IEnumerable<BrunoExtractResult> Extract([NotNull] BrunoExpression expression,
                                                              [NotNull] Func<BrunoExpression, bool> predicate)
            => new ExtractVisitor(expression, predicate).Extract();
    }

    public class BrunoExtractResult
    {
        public BrunoExtractResult(BrunoExpression parent, int depth, [NotNull] BrunoExpression node)
        {
            Parent = parent;
            Depth  = depth;
            Node   = node ?? throw new ArgumentNullException(nameof(node));
        }

        public int Depth { get; }

        public BrunoExpression Node { get; }

        public BrunoExpression Parent { get; }
    }
}