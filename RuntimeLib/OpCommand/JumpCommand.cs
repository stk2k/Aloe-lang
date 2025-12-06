// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.RuntimeLib;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class JumpCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            // 即値として相対オフセット(int32)を読む
            int offset = reader.ReadInt32();

            // BytecodeReader は Position プロパティだけを持つので、
            // 命令ポインタの更新は Position で行う。
            reader.Position += offset;
        }
    }
}
