// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class MulCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            var stack = vm.OperandStack;

            var right = stack.Pop();
            var left = stack.Pop();

            if (left.Kind == EnumValueKind.Int &&
                right.Kind == EnumValueKind.Int)
            {
                // TODO: オーバーフロー検出を入れたくなったら checked コンテキストを検討
                int result = left.AsInt() * right.AsInt();
                stack.Push(AloeValue.FromInt(result));
            }
            else
            {
                throw new VmException(
                    $"Mul is not supported for {left.Kind} and {right.Kind}.");
            }
        }
    }
}
