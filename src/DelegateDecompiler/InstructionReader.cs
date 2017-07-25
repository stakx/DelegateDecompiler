#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

using System;
using System.Reflection;

/// <summary>
/// Contains functionality responsible for decoding an intermediate language (IL) byte stream.
/// </summary>
internal static partial class InstructionReader
{
    /// <summary>
    /// Decodes instructions in the given IL byte stream and sends them
    /// to a specified sink (callback interface).
    /// </summary>
    /// <param name="il">The IL byte stream to decode.</param>
    /// <param name="module">The <see cref="Module"/> used to resolve metadata tokens encountered in the IL.</param>
    /// <param name="sink">Callback interface that will receive the decoded instructions.</param>
    public static void Read(byte[] il, Module module, InstructionSink sink)
    {
        int metadataToken;
        byte byteValue;
        int intValue;
        long longValue;
        float floatValue;
        double doubleValue;

        for(int ip = 0;;)
        {
            var opCode = (OpCode)il[ip];
            ++ip;

            // check whether this op-code takes up two bytes:
            if (opCode == (OpCode)0xfe)
            {
                opCode = (OpCode)(((byte)opCode << 8) | il[ip]);
                ++ip;
            }

            switch (opCode)
            {
                case OpCode.add:
                    sink.add();
                    break;

                case OpCode.and:
                    sink.and();
                    break;

                case OpCode.box:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.box(module.ResolveType(metadataToken));
                    break;

                case OpCode.call:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.call(module.ResolveMethod(metadataToken));
                    break;

                case OpCode.callvirt:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.callvirt(module.ResolveMethod(metadataToken));
                    break;

                case OpCode.ceq:
                    sink.ceq();
                    break;

                case OpCode.cgt:
                    sink.cgt();
                    break;

                case OpCode.cgt_un:
                    sink.cgt_un();
                    break;

                case OpCode.clt:
                    sink.clt();
                    break;

                case OpCode.conv_i1:
                    sink.conv_i1();
                    break;

                case OpCode.conv_i2:
                    sink.conv_i2();
                    break;

                case OpCode.conv_i4:
                    sink.conv_i4();
                    break;

                case OpCode.conv_i8:
                    sink.conv_i8();
                    break;

                case OpCode.conv_r4:
                    sink.conv_r4();
                    break;

                case OpCode.conv_r8:
                    sink.conv_r8();
                    break;

                case OpCode.conv_u1:
                    sink.conv_u1();
                    break;

                case OpCode.conv_u2:
                    sink.conv_u2();
                    break;

                case OpCode.conv_u4:
                    sink.conv_u4();
                    break;

                case OpCode.conv_u8:
                    sink.conv_u8();
                    break;

                case OpCode.div:
                    sink.div();
                    break;

                case OpCode.dup:
                    sink.dup();
                    break;

                case OpCode.isinst:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.isinst(module.ResolveType(metadataToken));
                    break;

                case OpCode.ldarg_0:
                    sink.ldarg_0();
                    break;

                case OpCode.ldarg_1:
                    sink.ldarg_1();
                    break;

                case OpCode.ldarg_2:
                    sink.ldarg_2();
                    break;

                case OpCode.ldarg_3:
                    sink.ldarg_3();
                    break;

                case OpCode.ldarg_s:
                    byteValue = il[ip];
                    ++ip;
                    sink.ldarg_s(byteValue);
                    break;

                case OpCode.ldc_i4:
                    intValue = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.ldc_i4(intValue);
                    break;

                case OpCode.ldc_i4_0:
                    sink.ldc_i4_0();
                    break;

                case OpCode.ldc_i4_1:
                    sink.ldc_i4_1();
                    break;

                case OpCode.ldc_i4_2:
                    sink.ldc_i4_2();
                    break;

                case OpCode.ldc_i4_3:
                    sink.ldc_i4_3();
                    break;

                case OpCode.ldc_i4_4:
                    sink.ldc_i4_4();
                    break;

                case OpCode.ldc_i4_5:
                    sink.ldc_i4_5();
                    break;

                case OpCode.ldc_i4_6:
                    sink.ldc_i4_6();
                    break;

                case OpCode.ldc_i4_7:
                    sink.ldc_i4_7();
                    break;

                case OpCode.ldc_i4_8:
                    sink.ldc_i4_8();
                    break;

                case OpCode.ldc_i4_m1:
                    sink.ldc_i4_m1();
                    break;

                case OpCode.ldc_i4_s:
                    byteValue = il[ip];
                    ++ip;
                    sink.ldc_i4_s(byteValue);
                    break;

                case OpCode.ldc_i8:
                    longValue = BitConverter.ToInt64(il, ip);
                    ip += 8;
                    sink.ldc_i8(longValue);
                    break;

                case OpCode.ldc_r4:
                    floatValue = BitConverter.ToSingle(il, ip);
                    ip += 4;
                    sink.ldc_r4(floatValue);
                    break;

                case OpCode.ldc_r8:
                    doubleValue = BitConverter.ToDouble(il, ip);
                    ip += 8;
                    sink.ldc_r8(doubleValue);
                    break;

                case OpCode.ldelem_ref:
                    sink.ldelem_ref();
                    break;

                case OpCode.ldfld:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.ldfld((FieldInfo)module.ResolveField(metadataToken));
                    break;

                case OpCode.ldflda:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.ldflda((FieldInfo)module.ResolveField(metadataToken));
                    break;

                case OpCode.ldlen:
                    sink.ldlen();
                    break;

                case OpCode.ldloca_s:
                    byteValue = il[ip];
                    ++ip;
                    sink.ldloca_s(byteValue);
                    break;

                case OpCode.ldloc_0:
                    sink.ldloc_0();
                    break;

                case OpCode.ldloc_1:
                    sink.ldloc_1();
                    break;

                case OpCode.ldloc_2:
                    sink.ldloc_2();
                    break;

                case OpCode.ldloc_3:
                    sink.ldloc_3();
                    break;

                case OpCode.ldnull:
                    sink.ldnull();
                    break;

                case OpCode.ldsfld:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.ldflda((FieldInfo)module.ResolveField(metadataToken));
                    break;

                case OpCode.ldstr:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.ldstr(module.ResolveString(metadataToken));
                    break;

                case OpCode.mul:
                    sink.mul();
                    break;

                case OpCode.neg:
                    sink.neg();
                    break;

                case OpCode.newarr:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.newarr(module.ResolveType(metadataToken));
                    break;

                case OpCode.newobj:
                    metadataToken = BitConverter.ToInt32(il, ip);
                    ip += 4;
                    sink.newobj((ConstructorInfo)module.ResolveMethod(metadataToken));
                    break;

                case OpCode.nop:
                    sink.nop();
                    break;

                case OpCode.not:
                    sink.not();
                    break;

                case OpCode.or:
                    sink.or();
                    break;

                case OpCode.pop:
                    sink.pop();
                    break;

                case OpCode.rem:
                    sink.rem();
                    break;

                case OpCode.ret:
                    sink.ret();
                    return;

                case OpCode.stloc_0:
                    sink.stloc_0();
                    break;

                case OpCode.stloc_1:
                    sink.stloc_1();
                    break;

                case OpCode.stloc_2:
                    sink.stloc_2();
                    break;

                case OpCode.stloc_3:
                    sink.stloc_3();
                    break;

                case OpCode.stloc_s:
                    byteValue = il[ip];
                    ++ip;
                    sink.stloc_s(byteValue);
                    break;

                case OpCode.sub:
                    sink.sub();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
