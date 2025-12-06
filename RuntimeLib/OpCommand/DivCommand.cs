// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class DivCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            var stack = vm.OperandStack;

            var right = stack.Pop();
            var left = stack.Pop();

            if (left.Kind == EnumValueKind.Int &&
                right.Kind == EnumValueKind.Int)
            {
                int rhs = right.AsInt();
                if (rhs == 0)
                {
                    // TODO: 仕様に合わせて ZeroDivisionException など専用例外を実装して差し替えてもよい
                    throw new VmException("Division by zero.");
                }

                int result = left.AsInt() / rhs;
                stack.Push(AloeValue.FromInt(result));
            }
            else
            {
                throw new VmException(
                    $"Div is not supported for {left.Kind} and {right.Kind}.");
            }
        }
    }
}
