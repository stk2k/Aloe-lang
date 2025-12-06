// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class PushConstCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            int constIndex = reader.ReadInt32();
            var module = vm.Module;

            if (constIndex < 0 || constIndex >= module.Constants.Length)
            {
                throw new VmException($"Invalid constant index: {constIndex}");
            }

            var value = module.Constants[constIndex];
            vm.OperandStack.Push(value);
        }
    }
}
