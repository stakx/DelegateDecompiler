#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Reduces expression trees by evaluating all nodes that can be
/// evaluated. This can be used e.g. to evaluate captured variables,
/// reduce method calls to their return values, etc. Expressions
/// that involve a lambda parameter cannot are irreducible.
/// </summary>
public sealed class PartialEvaluator : ExpressionVisitor
{
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is ConstantExpression ce && node.Member is FieldInfo fi)
        {
            return Expression.Constant(fi.GetValue(ce.Value), fi.FieldType);
        }

        return base.VisitMember(node);
    }
}
