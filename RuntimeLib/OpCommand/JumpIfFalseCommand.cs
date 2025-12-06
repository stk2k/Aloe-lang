// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class JumpIfFalseCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            // 1. 先に相対オフセット(int32)を読む
            int offset = reader.ReadInt32();

            var stack = vm.OperandStack;
            var condition = stack.Pop();

            if (condition.Kind != EnumValueKind.Bool)
            {
                throw new VmException(
                    $"JumpIfFalse requires bool, but got {condition.Kind}.");
            }

            // 2. false のときだけジャンプする
            if (!condition.AsBool())
            {
                // BytecodeReader は Position プロパティで命令ポインタを管理する
                reader.Position += offset;
            }
            // true の場合は何もしない（そのまま次の命令へ）
        }
    }
}
