// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class StoreLocalCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            int localIndex = reader.ReadInt32();
            var frame = vm.CurrentFrame;

            if (localIndex < 0 || localIndex >= frame.Locals.Length)
            {
                throw new VmException($"Invalid local index: {localIndex}");
            }

            var value = vm.OperandStack.Pop();
            frame.Locals[localIndex] = value;
        }
    }
}
