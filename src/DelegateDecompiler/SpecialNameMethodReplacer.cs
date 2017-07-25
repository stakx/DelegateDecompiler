#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Replaces invocations of specially named methods (such as
/// property accessor methods, or static operator methods)
/// to semantically more correct expression tree nodes.
/// </summary>
public sealed class SpecialNameMethodReplacer : DecompilationExpressionVisitor
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
                var propertyName = methodName.Substring("get_".Length);
                if (node.Arguments.Count > 0)
                {
                    // Get accessors with arguments could be either indexers,
                    // or non-C# properties.
                    var defaultMemberAttr = (DefaultMemberAttribute)Attribute.GetCustomAttribute(method.DeclaringType, typeof(DefaultMemberAttribute));
                    if (defaultMemberAttr?.MemberName == propertyName)
                    {
                        // yes, it's an indexer!
                        return Expression.MakeIndex(Visit(node.Object), method.DeclaringType.GetProperty(propertyName), node.Arguments.Select(a => Visit(a)));
                    }
                    // TODO: support properties with arguments (I think VB.NET has those)
                }

                return Expression.Property(Visit(node.Object), propertyName);
            }
            else if (methodName.StartsWith("set_"))
            {
                // TODO: support multi-parameter properties and indexers
                var propertyName = methodName.Substring("set_".Length);
                if (node.Arguments.Count > 0)
                {
                    // Get accessors with arguments could be either indexers,
                    // or non-C# properties.
                    var defaultMemberAttr = (DefaultMemberAttribute)Attribute.GetCustomAttribute(method.DeclaringType, typeof(DefaultMemberAttribute));
                    if (defaultMemberAttr?.MemberName == propertyName)
                    {
                        // yes, it's an indexer!
                        return Expression.Assign(
                            Expression.MakeIndex(Visit(node.Object), method.DeclaringType.GetProperty(propertyName), node.Arguments.Take(node.Arguments.Count - 1).Select(a => Visit(a))),
                            Visit(node.Arguments.Last()));
                    }
                    // TODO: support properties with arguments (I think VB.NET has those)
                }

                return Expression.Assign(
                    Expression.Property(Visit(node.Object), propertyName),
                    Visit(node.Arguments.Last()));
            }
            else if (methodName.StartsWith("add_"))
            {
                var eventName = methodName.Substring("add_".Length);
                Debug.Assert(node.Arguments.Count == 1);
                return new EventAddExpression(
                    Visit(node.Object),
                    node.Object.Type.GetEvent(eventName),
                    Visit(node.Arguments[0]));
            }
            else if (methodName.StartsWith("remove_"))
            {
                var eventName = methodName.Substring("remove_".Length);
                Debug.Assert(node.Arguments.Count == 1);
                return new EventRemoveExpression(
                    Visit(node.Object),
                    node.Object.Type.GetEvent(eventName),
                    Visit(node.Arguments[0]));
            }
        }

        return base.VisitMethodCall(node);
    }
}
