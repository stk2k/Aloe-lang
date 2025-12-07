using System;
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;

namespace Aloe.RuntimeLib.OpCommand
{
    /// <summary>
    /// CALL 命令。
    /// 
    /// Instruction.Operand0 : 呼び出す関数のインデックス（Module.Functions のインデックス）
    /// 
    /// - スタックから引数をポップして、新しい CallFrame の Locals[0..ParameterCount-1] に詰める。
    /// - 新しい CallFrame を生成して vm.CallStack に Push する。
    /// - 呼び出し元フレームの IP は AloeVm.Run 側で既に次命令を指している前提。
    /// </summary>
    public sealed class CallCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, CallFrame frame, in Instruction instruction)
        {
            var module = vm.Module;
            var functions = module.Functions;

            // Operand0: 呼び出し先関数のインデックス
            int functionIndex = instruction.Operand0;

            if (functionIndex < 0 || functionIndex >= functions.Count)
            {
                throw new VmException(
                    $"CALL: function index out of range. index={functionIndex}, count={functions.Count}");
            }

            var callee = functions[functionIndex];

            int parameterCount = callee.ParameterCount;
            int localCount = callee.LocalCount;

            // LocalCount が ParameterCount 未満でも一応動くようにフォールバック
            if (localCount < parameterCount)
            {
                localCount = parameterCount;
            }

            // ローカル変数＋引数用スロットを確保
            AloeValue[] locals = localCount > 0
                ? new AloeValue[localCount]
                : Array.Empty<AloeValue>();

            // 引数を評価スタックから取り出して Locals[0..parameterCount-1] に格納
            // スタックには [ ... arg0, arg1, ..., argN-1 ] の順で積まれている前提で、
            // 最後に積まれた argN-1 から逆順に取り出す。
            for (int i = parameterCount - 1; i >= 0; i--)
            {
                locals[i] = vm.Pop();
            }

            // 関数エントリ IP
            int entryIp = callee.EntryIp;

            // 新しいコールフレームを生成してスタックに積む
            var newFrame = new CallFrame(
                module: module,
                function: callee,
                ip: entryIp,
                locals: locals
            );

            vm.CallStack.Push(newFrame);
        }
    }
}
