#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Reflection;

/// <summary>
/// A callback interface that will receive decoded IL instructions.
/// </summary>
internal abstract class InstructionSink
{
    public virtual void add() { }
    public virtual void and() { }
    public virtual void box(Type type) { }
    public virtual void call(MethodBase method) { }
    public virtual void callvirt(MethodBase method) { }
    public virtual void ceq() { }
    public virtual void cgt() { }
    public virtual void cgt_un() { }
    public virtual void clt() { }
    public virtual void conv_i1() { }
    public virtual void conv_i2() { }
    public virtual void conv_i4() { }
    public virtual void conv_i8() { }
    public virtual void conv_r4() { }
    public virtual void conv_r8() { }
    public virtual void conv_u1() { }
    public virtual void conv_u2() { }
    public virtual void conv_u4() { }
    public virtual void conv_u8() { }
    public virtual void div() { }
    public virtual void dup() { }
    public virtual void isinst(Type type) { }
    public virtual void ldarg_0() { }
    public virtual void ldarg_1() { }
    public virtual void ldarg_2() { }
    public virtual void ldarg_3() { }
    public virtual void ldarg_s(byte index) { }
    public virtual void ldc_i4(int value) { }
    public virtual void ldc_i4_0() { }
    public virtual void ldc_i4_1() { }
    public virtual void ldc_i4_2() { }
    public virtual void ldc_i4_3() { }
    public virtual void ldc_i4_4() { }
    public virtual void ldc_i4_5() { }
    public virtual void ldc_i4_6() { }
    public virtual void ldc_i4_7() { }
    public virtual void ldc_i4_8() { }
    public virtual void ldc_i4_m1() { }
    public virtual void ldc_i4_s(byte value) { }
    public virtual void ldc_i8(long value) { }
    public virtual void ldc_r4(float value) { }
    public virtual void ldc_r8(double value) { }
    public virtual void ldelem_ref() { }
    public virtual void ldfld(FieldInfo field) { }
    public virtual void ldflda(FieldInfo field) { }
    public virtual void ldlen() { }
    public virtual void ldloc_0() { }
    public virtual void ldloc_1() { }
    public virtual void ldloc_2() { }
    public virtual void ldloc_3() { }
    public virtual void ldloca_s(byte index) { }
    public virtual void ldnull() { }
    public virtual void ldsfld(FieldInfo field) { }
    public virtual void ldstr(string value) { }
    public virtual void mul() { }
    public virtual void neg() { }
    public virtual void newarr(Type type) { }
    public virtual void newobj(ConstructorInfo constructor) { }
    public virtual void nop() { }
    public virtual void not() { }
    public virtual void or() { }
    public virtual void pop() { }
    public virtual void rem() { }
    public virtual void ret() { }
    public virtual void stloc_0() { }
    public virtual void stloc_1() { }
    public virtual void stloc_2() { }
    public virtual void stloc_3() { }
    public virtual void stloc_s(byte index) { }
    public virtual void sub() { }
}
