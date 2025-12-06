// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.RuntimeLib;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class HaltCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            vm.Halt();
        }
    }
}
