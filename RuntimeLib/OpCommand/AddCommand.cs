// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib;
using Microsoft.VisualBasic;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class AddCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            var stack = vm.OperandStack;

            var right = stack.Pop();
            var left = stack.Pop();

            if (left.Kind == EnumValueKind.Int &&
                right.Kind == EnumValueKind.Int)
            {
                int result = left.AsInt() + right.AsInt();
                stack.Push(AloeValue.FromInt(result));
            }
            else
            {
                throw new VmException(
                    $"Add is not supported for {left.Kind} and {right.Kind}.");
            }
        }
    }
}
