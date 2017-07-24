#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// Replaces invocations of specially named methods (such as
/// property accessor methods, or static operator methods)
/// to semantically more correct expression tree nodes.
/// </summary>
public sealed class SpecialNameMethodReplacer : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var method = node.Method;
        if (method.IsSpecialName)
        {
            var methodName = method.Name;

            if (methodName.StartsWith("op_"))
            {
                switch (methodName)
                {
                    case "op_Equality":
                        return Expression.Equal(Visit(node.Arguments[0]), Visit(node.Arguments[1]));

                    case "op_Inequality":
                        return Expression.NotEqual(Visit(node.Arguments[0]), Visit(node.Arguments[1]));
                }
            }
            else if (methodName.StartsWith("get_"))
            {
                // TODO: support multi-parameter properties and indexers
                var propertyName = methodName.Substring("get_".Length);
                return Expression.Property(Visit(node.Object), propertyName);
            }
            else if (methodName.StartsWith("set_"))
            {
                // TODO: support multi-parameter properties and indexers
                var propertyName = methodName.Substring("set_".Length);
                Debug.Assert(node.Arguments.Count == 1);
                return Expression.Assign(
                    Expression.Property(Visit(node.Object), propertyName),
                    Visit(node.Arguments.Last()));
            }
        }

        return base.VisitMethodCall(node);
    }
}
