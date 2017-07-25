#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System.Linq.Expressions;
using System.Reflection;

public sealed class EventRemoveExpression : EventAccessorCallExpression
{
    public EventRemoveExpression(Expression instance, EventInfo @event, Expression handler)
        : base(instance, @event, handler)
    {
    }

    protected override AccessorKind Kind => AccessorKind.Remove;

    protected override Expression Accept(ExpressionVisitor visitor)
    {
        if (visitor is DecompilationExpressionVisitor extendedVisitor)
        {
            extendedVisitor.Visit(this);
        }

        return base.Accept(visitor);
    }

    public override string ToString()
    {
        return $"{this.Instance}.{this.Event.Name} -= {this.Handler}";
    }
}
