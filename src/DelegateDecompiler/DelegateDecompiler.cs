#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

public static partial class DelegateDecompiler
{
    /// <summary>
    /// Decompiles a lambda back into a LINQ expression tree.
    /// </summary>
    /// <param name="lambda">The lambda to be decompiled.</param>
    /// <remarks>
    /// Methods containing branches or exception handling are not supported.
    /// </remarks>
    public static LambdaExpression Decompile(this Delegate lambda)
    {
        var method = lambda.Method;
        var methodBody = method.GetMethodBody();

        // We will need a reference to the module containing the method in order to
        // resolve metadata tokens encountered inside the IL.
        var module = method.Module;

        // The implicit `this` argument (null for static methods).
        var thisValue = Expression.Constant(lambda.Target);

        // The method's actual parameters, never including the implicit `this` argument
        // of instance methods.
        var parameters = Array.ConvertAll(method.GetParameters(), ToParameterExpression);

        // The instruction `ldarg.1` will correspond to `parameters[0]` for instance
        // methods but to `parameters[1]` for static methods. The following offset will
        // be used to give us an index adjustment for `parameters`, given an `ldarg.n`
        // or `starg.n` instruction.
        var thisOffset = method.IsStatic ? 0 : -1;

        // Local variables of the method.
        var localVariableCount = methodBody.LocalVariables.Count;
        Expression[] localVariables = localVariableCount > 0 ? new Expression[localVariableCount] : null;
        if (methodBody.InitLocals)
        {
            for (int localVariableIndex = 0; localVariableIndex < localVariableCount; ++localVariableIndex)
            {
                var localVariableInfo = methodBody.LocalVariables[localVariableIndex];
                localVariables[localVariableIndex] = Expression.Constant(GetDefaultValue(localVariableInfo.LocalType), localVariableInfo.LocalType);
            }
        }

        // The execution stack. Instead of putting values onto this stack, we will be
        // pushing expressions representing the values.
        var stack = new ArrayStack<Expression>(capacity: methodBody.MaxStackSize);

        // The IL and instruction pointer.
        var il = methodBody.GetILAsByteArray();
        var ip = 0;

        for (;;)
        {
            var opCode = (OpCode)il[ip];
            ++ip;

            switch (opCode)
            {
                case OpCode.add:
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(Expression.Add(left, right));
                        break;
                    }

                case OpCode.box:
                    {
                        // The `box` op-code is followed by a metadata token specifying
                        // the type of object being boxed. Boxing operations are too low-
                        // level for expression trees, so we throw this away and achieve
                        // boxing simply by converting to type `object`.
                        var ignoredMetadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;

                        stack.Push(Expression.Convert(stack.Pop(), typeof(object)));

                        break;
                    }

                case OpCode.call:
                case OpCode.callvirt:
                    {
                        var metadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        var target = (MethodInfo)module.ResolveMethod(metadataToken);
                        var targetParameters = target.GetParameters();
                        var targetParameterCount = targetParameters.Length;
                        var arguments = stack.Pop(targetParameterCount);

                        // TODO: The following is not thoroughly tested. The intention here is
                        // to make the arguments compatible with target method's signature,
                        // which is not a given since the VES execution stack doesn't use the
                        // full range of types, but only a simplified set.
                        for (int targetParameterIndex = 0; targetParameterIndex < targetParameterCount; ++targetParameterIndex)
                        {
                            if (arguments[targetParameterIndex] is ConstantExpression ce && !targetParameters[targetParameterIndex].ParameterType.IsAssignableFrom(ce.Type))
                            {
                                arguments[targetParameterIndex] = Coerce(ce, targetParameters[targetParameterIndex].ParameterType);
                            }
                        }

                        var targetInstance = target.IsStatic ? null : stack.Pop();
                        stack.Push(Expression.Call(targetInstance, target, arguments));
                        break;
                    }

                case OpCode.conv_i1:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(sbyte)));
                        break;
                    }

                case OpCode.conv_i2:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(short)));
                        break;
                    }

                case OpCode.conv_i4:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(int)));
                        break;
                    }

                case OpCode.conv_i8:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(long)));
                        break;
                    }

                case OpCode.conv_r4:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(float)));
                        break;
                    }

                case OpCode.conv_r8:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(double)));
                        break;
                    }

                case OpCode.conv_u1:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(byte)));
                        break;
                    }

                case OpCode.conv_u2:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(ushort)));
                        break;
                    }

                case OpCode.conv_u4:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(uint)));
                        break;
                    }

                case OpCode.conv_u8:
                    {
                        var value = stack.Pop();
                        stack.Push(Expression.Convert(value, typeof(ulong)));
                        break;
                    }

                case OpCode.div:
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(Expression.Divide(left, right));
                        break;
                    }

                case OpCode.dup:
                    stack.Push(stack.Peek());
                    break;

                case OpCode.ldarg_0:
                    if (method.IsStatic)
                    {
                        stack.Push(parameters[0]);
                    }
                    else
                    {
                        stack.Push(thisValue);
                    }
                    break;

                case OpCode.ldarg_1:
                    stack.Push(parameters[1 + thisOffset]);
                    break;

                case OpCode.ldarg_2:
                    stack.Push(parameters[2 + thisOffset]);
                    break;

                case OpCode.ldarg_3:
                    stack.Push(parameters[3 + thisOffset]);
                    break;

                case OpCode.ldarg_s:
                    {
                        int index = il[ip];
                        ++ip;
                        if (ip > 0 || method.IsStatic)
                        {
                            stack.Push(parameters[index + thisOffset]);
                        }
                        else
                        {
                            stack.Push(thisValue);
                        }
                        break;
                    }

                case OpCode.ldc_i4:
                    {
                        var value = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        stack.Push(Expression.Constant(value, typeof(int)));
                        break;
                    }

                case OpCode.ldc_i4_0:
                    stack.Push(Expression.Constant(0, typeof(int)));
                    break;

                case OpCode.ldc_i4_1:
                    stack.Push(Expression.Constant(1, typeof(int)));
                    break;

                case OpCode.ldc_i4_2:
                    stack.Push(Expression.Constant(2, typeof(int)));
                    break;

                case OpCode.ldc_i4_3:
                    stack.Push(Expression.Constant(3, typeof(int)));
                    break;

                case OpCode.ldc_i4_4:
                    stack.Push(Expression.Constant(4, typeof(int)));
                    break;

                case OpCode.ldc_i4_5:
                    stack.Push(Expression.Constant(5, typeof(int)));
                    break;

                case OpCode.ldc_i4_6:
                    stack.Push(Expression.Constant(6, typeof(int)));
                    break;

                case OpCode.ldc_i4_7:
                    stack.Push(Expression.Constant(7, typeof(int)));
                    break;

                case OpCode.ldc_i4_8:
                    stack.Push(Expression.Constant(8, typeof(int)));
                    break;

                case OpCode.ldc_i4_m1:
                    stack.Push(Expression.Constant(-1, typeof(int)));
                    break;

                case OpCode.ldc_i4_s:
                    {
                        var constant = (int)il[ip];
                        ++ip;
                        stack.Push(Expression.Constant(constant, typeof(int)));
                        break;
                    }

                case OpCode.ldc_i8:
                    {
                        var value = BitConverter.ToInt64(il, ip);
                        ip += 8;
                        stack.Push(Expression.Constant(value, typeof(long)));
                        break;
                    }

                case OpCode.ldc_r4:
                    {
                        var value = BitConverter.ToSingle(il, ip);
                        ip += 4;
                        stack.Push(Expression.Constant(value, typeof(float)));
                        break;
                    }

                case OpCode.ldc_r8:
                    {
                        var value = BitConverter.ToDouble(il, ip);
                        ip += 8;
                        stack.Push(Expression.Constant(value, typeof(double)));
                        break;
                    }

                case OpCode.ldelem_ref:
                    {
                        var index = stack.Pop();
                        var array = stack.Pop();
                        stack.Push(Expression.ArrayIndex(array, index));
                        break;
                    }

                case OpCode.ldfld:
                case OpCode.ldflda:
                    {
                        var metadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        var field = module.ResolveField(metadataToken);
                        var instance = stack.Pop();
                        stack.Push(Expression.MakeMemberAccess(instance, field));
                        break;
                    }

                case OpCode.ldloc_0:
                    stack.Push(localVariables[0]);
                    break;

                case OpCode.ldloc_1:
                    stack.Push(localVariables[1]);
                    break;

                case OpCode.ldloc_2:
                    stack.Push(localVariables[2]);
                    break;

                case OpCode.ldloc_3:
                    stack.Push(localVariables[3]);
                    break;

                case OpCode.ldloca_s:
                    {
                        var localsIndex = il[ip];
                        ++ip;
                        stack.Push(localVariables[localsIndex]);
                        break;
                    }

                case OpCode.ldnull:
                    stack.Push(Expression.Constant(null));
                    break;

                case OpCode.ldsfld:
                    {
                        var metadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        var fieldToLoad = module.ResolveField(metadataToken);
                        stack.Push(Expression.MakeMemberAccess(null, fieldToLoad));
                        break;
                    }

                case OpCode.ldstr:
                    {
                        var metadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        var str = module.ResolveString(metadataToken);
                        stack.Push(Expression.Constant(str));
                        break;
                    }

                case OpCode.mul:
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(Expression.Multiply(left, right));
                        break;
                    }


                case OpCode.newarr:
                    {
                        var metadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        var elementType = module.ResolveType(metadataToken);
                        var length = stack.Pop();
                        stack.Push(Expression.NewArrayBounds(elementType, length));
                        break;
                    }

                case OpCode.newobj:
                    {
                        var metadataToken = BitConverter.ToInt32(il, ip);
                        ip += 4;
                        var ctor = (ConstructorInfo)module.ResolveMethod(metadataToken);
                        var ctorParameters = ctor.GetParameters();
                        var ctorParameterCount = ctorParameters.Length;
                        var arguments = stack.Pop(ctorParameterCount);

                        // TODO: The following is not thoroughly tested. The intention here is
                        // to make the arguments compatible with target method's signature,
                        // which is not a given since the VES execution stack doesn't use the
                        // full range of types, but only a simplified set.
                        for (int ctorParameterIndex = 0; ctorParameterIndex < ctorParameterCount; ++ctorParameterIndex)
                        {
                            if (arguments[ctorParameterIndex] is ConstantExpression ce && !ctorParameters[ctorParameterIndex].ParameterType.IsAssignableFrom(ce.Type))
                            {
                                arguments[ctorParameterIndex] = Coerce(ce, ctorParameters[ctorParameterIndex].ParameterType);
                            }
                        }

                        stack.Push(Expression.New(ctor, arguments));
                        break;
                    }

                case OpCode.nop:
                    break;

                case OpCode.rem:
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(Expression.Modulo(left, right));
                        break;
                    }

                case OpCode.ret:
                    return Expression.Lambda(delegateType: lambda.GetType(), body: stack.Pop(), parameters: parameters);

                case OpCode.stloc_0:
                    localVariables[0] = stack.Pop();
                    break;

                case OpCode.stloc_1:
                    localVariables[1] = stack.Pop();
                    break;

                case OpCode.stloc_2:
                    localVariables[2] = stack.Pop();
                    break;

                case OpCode.stloc_3:
                    localVariables[3] = stack.Pop();
                    break;

                case OpCode.stloc_s:
                    {
                        var localsIndex = il[ip];
                        ++ip;
                        localVariables[localsIndex] = stack.Pop();
                        break;
                    }

                case OpCode.sub:
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(Expression.Subtract(left, right));
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }

    private static ConstantExpression Coerce(ConstantExpression constant, Type type)
    {
        // TODO: this is too simplistic and doesn't work in many cases,
        // such as when converting an integer constant to a `char`.
        return Expression.Constant(constant.Value, type);
    }

    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static ParameterExpression ToParameterExpression(this ParameterInfo parameter)
    {
        return Expression.Parameter(parameter.ParameterType, parameter.Name);
    }
}
