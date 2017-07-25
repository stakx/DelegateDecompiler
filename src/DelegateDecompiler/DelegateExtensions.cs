#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Linq.Expressions;

public static class DelegateExtensions
{
    /// <summary>
    /// Decompiles the given delegate into a LINQ expression tree.
    /// </summary>
    /// <param name="lambda">The delegate to decompile.</param>
    /// <returns>A <see cref="LambdaExpression"/> representing the delegate's decompiled method.</returns>
    public static LambdaExpression Decompile(this Delegate lambda)
    {
        var method = lambda.Method;
        var methodBody = method.GetMethodBody();

        var decompilationContext = new DecompilationContext(lambda, method, methodBody);

        var il = methodBody.GetILAsByteArray();
        InstructionReader.Read(il, method.Module, decompilationContext);

        return decompilationContext.Result;
    }
}
