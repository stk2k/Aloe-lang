// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class LoadLocalCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            int localIndex = reader.ReadInt32();

            // 現在のフレームを ref で取得
            var frame = vm.CurrentFrame;

            if (localIndex < 0 || localIndex >= frame.Locals.Length)
            {
                throw new VmException($"Invalid local index: {localIndex}");
            }

            // ローカルから値を取り出してオペランドスタックへ積む
            vm.OperandStack.Push(frame.Locals[localIndex]);
        }
    }
}
