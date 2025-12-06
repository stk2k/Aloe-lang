// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class CallCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            // 1) バイトコードから関数インデックスと引数個数を読む
            int functionIndex = reader.ReadInt32();
            int argCount = reader.ReadInt32();

            var module = vm.Module;
            var stack = vm.OperandStack;
            var callStack = vm.CallStack;

            if (functionIndex < 0 || functionIndex >= module.Functions.Count)
            {
                throw new VmException($"Invalid function index: {functionIndex}");
            }

            var function = module.Functions[functionIndex];

            // パラメータ数チェック（仕様に合わせてゆるく／厳しく）
            if (argCount != function.ParameterCount)
            {
                throw new VmException(
                    $"Function '{function.Name}' expects {function.ParameterCount} arguments, " +
                    $"but {argCount} were supplied.");
            }

            if (argCount > stack.Count)
            {
                throw new VmException(
                    $"Stack underflow: Call requires {argCount} arguments but stack contains only {stack.Count} values.");
            }

            // 2) スタックから引数を取り出す（args[0] が最初の引数になるように）
            var args = new AloeValue[argCount];
            for (int i = argCount - 1; i >= 0; i--)
            {
                args[i] = stack.Pop();
            }

            // 3) 新しい CallFrame を作成
            //    - returnAddress: 現在の reader.Position（＝次の命令）
            //    - ip: 呼び出し先関数のコード開始位置
            var frame = new CallFrame(
                module: module,
                function: function,
                ip: function.CodeOffset,
                returnAddress: reader.Position,
                localCount: function.LocalCount
            );

            // 4) ローカルに引数をコピー
            //    ここでは「先頭 local がパラメータ領域」という前提
            for (int i = 0; i < argCount; i++)
            {
                frame.Locals[i] = args[i];
            }

            // 5) コールスタックにプッシュして、IP を遷移
            callStack.Push(frame);

            // VM 側の Reader も、呼び出し先関数のコード位置に飛ばす
            vm.Reader.Position = function.CodeOffset;
        }
    }
}
