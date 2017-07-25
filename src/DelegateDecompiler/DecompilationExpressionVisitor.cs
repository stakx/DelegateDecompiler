#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System.Linq.Expressions;

/// <summary>
/// Base class for expression tree visitors that understand event accessors.
/// </summary>
public abstract class DecompilationExpressionVisitor : ExpressionVisitor
{
    public virtual Expression Visit(EventAddExpression node)
    {
        return node;
    }

    public virtual Expression Visit(EventRemoveExpression node)
    {
        return node;
    }
}
