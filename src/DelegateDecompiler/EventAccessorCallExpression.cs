#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

public abstract class EventAccessorCallExpression : Expression
{
    private Expression instance;
    private EventInfo @event;
    private Expression handler;

    internal EventAccessorCallExpression(Expression instance, EventInfo @event, Expression handler)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        this.instance = instance;

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (@event.DeclaringType != instance.Type)
        {
            throw new ArgumentException($"Event not declared on type of given instance.", nameof(@event));
        }

        this.@event = @event;

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (!typeof(Delegate).IsAssignableFrom(handler.Type))
        {
            throw new ArgumentException($"Not a delegate.", nameof(handler));
        }

        this.handler = handler;
    }

    public Expression Instance => this.instance;

    public EventInfo Event => this.@event;

    public Expression Handler => this.handler;

    public override bool CanReduce => true;

    public override ExpressionType NodeType => ExpressionType.Call;

    public override Type Type => typeof(void);

    protected abstract AccessorKind Kind { get; }

    public override bool Equals(object obj)
    {
        return obj is EventAccessorCallExpression other
            && other.Kind == this.Kind
            && other.instance.Equals(this.instance)
            && other.@event == this.@event
            && other.handler.Equals(this.handler);
    }

    public override int GetHashCode()
    {
        return unchecked(
              this.instance.GetHashCode()
            + 3 * this.Kind.GetHashCode()
            + 7 * this.@event.GetHashCode()
            + 17 * this.handler.GetHashCode());
    }

    public override Expression Reduce()
    {
        return Expression.Call(
            this.instance,
            this.@event.GetAddMethod(),
            this.handler);
    }

    public override abstract string ToString();

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var returnedInstance = visitor.Visit(this.instance);
        var returnedHandler = visitor.Visit(this.handler);

        if (returnedInstance == this.instance && returnedHandler == this.handler)
        {
            return this;
        }
        else
        {
            return new EventAddExpression(returnedInstance, this.@event, returnedHandler);
        }
    }

    protected enum AccessorKind
    {
        Add,
        Remove,
    }
}
