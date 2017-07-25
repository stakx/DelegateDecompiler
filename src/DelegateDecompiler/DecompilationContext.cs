#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Contains the data used during, as well as the result of, a single decompilation process.
/// </summary>
internal sealed class DecompilationContext : InstructionSink
{
    private Delegate lambda;
    private MethodInfo method;
    private Expression thisValue;
    private int thisOffset;
    private ParameterExpression[] parameters;
    private Expression[] localVariables;
    private ArrayStack<Expression> stack;
    private LambdaExpression result;

    public DecompilationContext(Delegate lambda, MethodInfo method, MethodBody methodBody)
    {
        this.lambda = lambda;
        this.method = method;

        // The implicit `this` argument (null for static methods).
        this.thisValue = Expression.Constant(lambda.Target);

        // The method's actual parameters, never including the implicit `this` argument
        // of instance methods.
        this.parameters = Array.ConvertAll(method.GetParameters(), ToParameterExpression);

        // The instruction `ldarg.1` will correspond to `parameters[0]` for instance
        // methods but to `parameters[1]` for static methods. The following offset will
        // be used to give us an index adjustment for `parameters`, given an `ldarg.n`
        // or `starg.n` instruction.
        this.thisOffset = method.IsStatic ? 0 : -1;

        // Local variables of the method.
        var localVariableCount = methodBody.LocalVariables.Count;
        this.localVariables = localVariableCount > 0 ? new Expression[localVariableCount] : null;
        if (methodBody.InitLocals)
        {
            for (int localVariableIndex = 0; localVariableIndex < localVariableCount; ++localVariableIndex)
            {
                var localVariableInfo = methodBody.LocalVariables[localVariableIndex];
                this.localVariables[localVariableIndex] = Expression.Constant(GetDefaultValue(localVariableInfo.LocalType), localVariableInfo.LocalType);
            }
        }

        // The execution stack. Instead of putting values onto this stack, we will be
        // pushing expressions representing the values.
        this.stack = new ArrayStack<Expression>(capacity: methodBody.MaxStackSize);
    }

    public LambdaExpression Result => this.result ?? throw new InvalidOperationException();

    public override void add() => this.OnAnyBinary(ExpressionType.Add);

    public override void box(Type type)
    {
        this.stack.Push(Expression.Convert(this.stack.Pop(), typeof(object)));
    }

    public override void call(MethodBase method)
        => this.OnAnyCall((MethodInfo)method);

    public override void callvirt(MethodBase method)
        => this.OnAnyCall((MethodInfo)method);

    private void OnAnyCall(MethodInfo target)
    {
        var targetParameters = target.GetParameters();
        var targetParameterCount = targetParameters.Length;
        var arguments = this.stack.Pop(targetParameterCount);

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

        var targetInstance = target.IsStatic ? null : this.stack.Pop();
        this.stack.Push(Expression.Call(targetInstance, target, arguments));
    }

    public override void conv_i1() => this.OnAnyConv(typeof(sbyte));

    public override void conv_i2() => this.OnAnyConv(typeof(short));

    public override void conv_i4() => this.OnAnyConv(typeof(int));

    public override void conv_i8() => this.OnAnyConv(typeof(long));

    public override void conv_r4() => this.OnAnyConv(typeof(float));

    public override void conv_r8() => this.OnAnyConv(typeof(double));

    public override void conv_u1() => this.OnAnyConv(typeof(byte));

    public override void conv_u2() => this.OnAnyConv(typeof(ushort));

    public override void conv_u4() => this.OnAnyConv(typeof(uint));

    public override void conv_u8() => this.OnAnyConv(typeof(ulong));

    private void OnAnyConv(Type type)
    {
        var value = this.stack.Pop();
        this.stack.Push(Expression.Convert(value, type));
    }

    public override void div() => this.OnAnyBinary(ExpressionType.Divide);

    public override void dup()
    {
        this.stack.Push(this.stack.Peek());
    }

    public override void ldarg_0()
    {
        if (this.method.IsStatic)
        {
            this.OnAnyLdarg(0);
        }
        else
        {
            this.stack.Push(this.thisValue);
        }
    }

    public override void ldarg_1() => this.OnAnyLdarg(1);

    public override void ldarg_2() => this.OnAnyLdarg(2);

    public override void ldarg_3() => this.OnAnyLdarg(3);

    public override void ldarg_s(byte index)
    {
        if (index > 0 || method.IsStatic)
        {
            this.OnAnyLdarg(index);
        }
        else
        {
            this.stack.Push(this.thisValue);
        }
    }

    private void OnAnyLdarg(byte index)
    {
        this.stack.Push(this.parameters[1 + this.thisOffset]);
    }

    public override void ldc_i4(int value) => this.OnAnyLdcI(value, typeof(int));

    public override void ldc_i4_0() => this.OnAnyLdcI(0, typeof(int));

    public override void ldc_i4_1() => this.OnAnyLdcI(1, typeof(int));

    public override void ldc_i4_2() => this.OnAnyLdcI(2, typeof(int));

    public override void ldc_i4_3() => this.OnAnyLdcI(3, typeof(int));

    public override void ldc_i4_4() => this.OnAnyLdcI(4, typeof(int));

    public override void ldc_i4_5() => this.OnAnyLdcI(5, typeof(int));

    public override void ldc_i4_6() => this.OnAnyLdcI(6, typeof(int));

    public override void ldc_i4_7() => this.OnAnyLdcI(7, typeof(int));

    public override void ldc_i4_8() => this.OnAnyLdcI(8, typeof(int));

    public override void ldc_i4_m1() => this.OnAnyLdcI(-1, typeof(int));

    public override void ldc_i4_s(byte value) => this.OnAnyLdcI(value, typeof(int));

    private void OnAnyLdcI(int value, Type type)
    {
        this.stack.Push(Expression.Constant(value, type));
    }

    public override void ldc_i8(long value)
    {
        this.stack.Push(Expression.Constant(value, typeof(long)));
    }

    public override void ldc_r4(float value)
    {
        this.stack.Push(Expression.Constant(value, typeof(float)));
    }

    public override void ldc_r8(double value)
    {
        this.stack.Push(Expression.Constant(value, typeof(double)));
    }

    public override void ldelem_ref()
    {
        var index = this.stack.Pop();
        var array = this.stack.Pop();
        this.stack.Push(Expression.ArrayIndex(array, index));
    }

    public override void ldfld(FieldInfo field)
    {
        var instance = this.stack.Pop();
        this.stack.Push(Expression.MakeMemberAccess(instance, field));
    }

    public override void ldflda(FieldInfo field)
    {
        var instance = this.stack.Pop();
        this.stack.Push(Expression.MakeMemberAccess(instance, field));
    }

    public override void ldloc_0()
        => this.OnAnyLdloc(0);

    public override void ldloc_1()
        => this.OnAnyLdloc(1);

    public override void ldloc_2()
        => this.OnAnyLdloc(2);

    public override void ldloc_3()
        => this.OnAnyLdloc(3);

    public override void ldloca_s(byte index)
        => this.OnAnyLdloc(index);

    private void OnAnyLdloc(byte index)
    {
        this.stack.Push(this.localVariables[index]);
    }

    public override void ldnull()
    {
        this.stack.Push(Expression.Constant(null));
    }

    public override void ldsfld(FieldInfo field)
    {
        this.stack.Push(Expression.MakeMemberAccess(null, field));
    }

    public override void ldstr(string value)
    {
        this.stack.Push(Expression.Constant(value));
    }

    public override void mul() => this.OnAnyBinary(ExpressionType.Multiply);

    public override void newarr(Type type)
    {
        var length = this.stack.Pop();
        this.stack.Push(Expression.NewArrayBounds(type, length));
    }

    public override void newobj(ConstructorInfo constructor)
    {
        var ctorParameters = constructor.GetParameters();
        var ctorParameterCount = ctorParameters.Length;
        var arguments = this.stack.Pop(ctorParameterCount);

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

        this.stack.Push(Expression.New(constructor, arguments));
    }

    public override void rem() => this.OnAnyBinary(ExpressionType.Modulo);

    public override void ret()
    {
        this.result = Expression.Lambda(delegateType: this.lambda.GetType(), body: this.stack.Pop(), parameters: this.parameters);
    }

    public override void stloc_0() => this.OnAnyStloc(0);

    public override void stloc_1() => this.OnAnyStloc(1);

    public override void stloc_2() => this.OnAnyStloc(2);

    public override void stloc_3() => this.OnAnyStloc(3);

    public override void stloc_s(byte index) => this.OnAnyStloc(index);

    private void OnAnyStloc(byte index)
    {
        this.localVariables[index] = this.stack.Pop();
    }

    public override void sub() => this.OnAnyBinary(ExpressionType.Subtract);

    private void OnAnyBinary(ExpressionType type)
    {
        var right = this.stack.Pop();
        var left = this.stack.Pop();
        this.stack.Push(Expression.MakeBinary(type, left, right));
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

    private static ParameterExpression ToParameterExpression(ParameterInfo parameter)
    {
        return Expression.Parameter(parameter.ParameterType, parameter.Name);
    }
}
