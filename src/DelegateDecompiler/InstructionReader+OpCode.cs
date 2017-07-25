﻿#region Copyright
// Copyright (c) 2017 stakx
// License available at https://github.com/stakx/DelegateDecompiler/blob/develop/LICENSE.md.
#endregion

partial class InstructionReader
{
    /// <summary>
    /// Recognized intermediate language (IL) op-codes.
    /// </summary>
    private enum OpCode : short
    {
        add = 0x58,
        box = 0x8c,
        call = 0x28,
        callvirt = 0x6f,
        conv_i1 = 0x67,
        conv_i2 = 0x68,
        conv_i4 = 0x69,
        conv_i8 = 0x6a,
        conv_r4 = 0x6b,
        conv_r8 = 0x6c,
        conv_u1 = 0xd2,
        conv_u2 = 0xd1,
        conv_u4 = 0x6d,
        conv_u8 = 0x6e,
        div = 0x5b,
        dup = 0x25,
        ldarg_0 = 0x02,
        ldarg_1 = 0x03,
        ldarg_2 = 0x04,
        ldarg_3 = 0x05,
        ldarg_s = 0x0e,
        ldc_i4 = 0x20,
        ldc_i4_0 = 0x16,
        ldc_i4_1 = 0x17,
        ldc_i4_2 = 0x18,
        ldc_i4_3 = 0x19,
        ldc_i4_4 = 0x1a,
        ldc_i4_5 = 0x1b,
        ldc_i4_6 = 0x1c,
        ldc_i4_7 = 0x1d,
        ldc_i4_8 = 0x1e,
        ldc_i4_m1 = 0x15,
        ldc_i4_s = 0x1f,
        ldc_i8 = 0x21,
        ldc_r4 = 0x22,
        ldc_r8 = 0x23,
        ldelem_ref = 0x9a,
        ldfld = 0x7b,
        ldflda = 0x7c,
        ldloc_0 = 0x06,
        ldloc_1 = 0x07,
        ldloc_2 = 0x08,
        ldloc_3 = 0x09,
        ldloca_s = 0x12,
        ldnull = 0x14,
        ldsfld = 0x7e,
        ldstr = 0x72,
        mul = 0x5a,
        newarr = 0x8d,
        newobj = 0x73,
        nop = 0x00,
        rem = 0x5d,
        ret = 0x2a,
        stloc_0 = 0x0a,
        stloc_1 = 0x0b,
        stloc_2 = 0x0c,
        stloc_3 = 0x0d,
        stloc_s = 0x13,
        sub = 0x59,
    }
}