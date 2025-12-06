using System;
using Aloe.CommonLib;

namespace Aloe.RuntimeLib
{
    /// <summary>
    /// 1つの Opcode に対応するコマンドインターフェイス。
    /// </summary>
    public interface IOpcodeCommand
    {
        /// <summary>
        /// 命令を実行する。
        /// 必要に応じて BytecodeReader から即値（int など）を読み取る。
        /// </summary>
        void Execute(AloeVm vm, BytecodeReader reader);
    }
}
