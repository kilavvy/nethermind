// SPDX-FileCopyrightText: 2025 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;
using System.Runtime.CompilerServices;

namespace Nethermind.Evm;
using Int256;
using Nethermind.GmpBindings;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal static partial class EvmInstructions
{
    public interface IOpMath3Param2
    {
        virtual static long GasCost => GasCostOf.Mid;
        abstract static void Operation(ref byte a, ref byte b, ref byte c, in Span<byte> result);
    }

    [SkipLocalsInit]
    public static EvmExceptionType InstructionMath3Param2<TOpMath, TTracingInst>(VirtualMachine _, ref EvmStack stack, ref long gasAvailable, ref int programCounter)
        where TOpMath : struct, IOpMath3Param2
        where TTracingInst : struct, IFlag
    {
        gasAvailable -= TOpMath.GasCost;

        ref byte a = ref stack.PopBytesByRef();

        if (Unsafe.IsNullRef(ref a))
            goto StackUnderflow;

        ref byte b = ref stack.PopBytesByRef();

        if (Unsafe.IsNullRef(ref b))
            goto StackUnderflow;

        ref byte c = ref stack.PopBytesByRef();

        if (Unsafe.IsNullRef(ref c))
            goto StackUnderflow;

        // Check for zero
        if (MemoryMarshal.CreateReadOnlySpan(ref c, 32).IndexOfAnyExcept((byte)0) == -1)
        {
            stack.PushZero<TTracingInst>();
        }
        else
        {
            Span<byte> result = stackalloc byte[32];
            TOpMath.Operation(ref a, ref b, ref c, result);
            stack.Push32Bytes<TTracingInst>(Vector256.Create<byte>(result));
        }

        return EvmExceptionType.None;
    StackUnderflow:
        // Jump forward to be unpredicted by the branch predictor
        return EvmExceptionType.StackUnderflow;
    }

    public struct OpAddMod2 : IOpMath3Param2
    {
        public static unsafe void Operation(ref byte a, ref byte b, ref byte c, in Span<byte> result)
        {
            using var aInt = mpz_t.Create();
            using var bInt = mpz_t.Create();
            using var cInt = mpz_t.Create();
            using var rInt = mpz_t.Create();

            fixed (byte* ptr = &a)
                Gmp.mpz_import(aInt, 32, 1, 1, 1, 1, (nint)ptr);

            fixed (byte* ptr = &b)
                Gmp.mpz_import(aInt, 32, 1, 1, 1, 1, (nint)ptr);

            fixed (byte* ptr = &c)
                Gmp.mpz_import(aInt, 32, 1, 1, 1, 1, (nint)ptr);

            Gmp.mpz_add(rInt, aInt, bInt);
            Gmp.mpz_mod(rInt, rInt, cInt);

            fixed (byte* ptr = &MemoryMarshal.GetReference(result))
                Gmp.mpz_export((nint)ptr, out _, 1, 1, 1, 1, rInt);
        }
    }

    public struct OpMulMod2 : IOpMath3Param2
    {
        public static unsafe void Operation(ref byte a, ref byte b, ref byte c, in Span<byte> result)
        {
            using var aInt = mpz_t.Create();
            using var bInt = mpz_t.Create();
            using var cInt = mpz_t.Create();
            using var rInt = mpz_t.Create();

            fixed (byte* ptr = &a)
                Gmp.mpz_import(aInt, 32, 1, 1, 1, 1, (nint)ptr);

            fixed (byte* ptr = &b)
                Gmp.mpz_import(aInt, 32, 1, 1, 1, 1, (nint)ptr);

            fixed (byte* ptr = &c)
                Gmp.mpz_import(aInt, 32, 1, 1, 1, 1, (nint)ptr);

            Gmp.mpz_mul(rInt, aInt, bInt);
            Gmp.mpz_mod(rInt, rInt, cInt);

            fixed (byte* ptr = &MemoryMarshal.GetReference(result))
                Gmp.mpz_export((nint)ptr, out _, 1, 1, 1, nuint.Zero, rInt);
        }
    }
}
