using System;

namespace Aloe.CommonLib
{
    public class CallFrame
    {
        /// <summary>このフレームが属するモジュール。</summary>
        public Module Module { get; }

        /// <summary>このフレームが実行している関数情報。</summary>
        public FunctionInfo Function { get; }

        /// <summary>現在の命令ポインタ（Module.Code 配列のインデックス）。</summary>
        public int Ip;

        /// <summary>
        /// 呼び出し元に戻る際の命令ポインタ。
        /// エントリ関数など「戻り先がない」場合は -1。
        /// </summary>
        public int ReturnAddress { get; }

        /// <summary>ローカル変数領域。</summary>
        public AloeValue[] Locals { get; }

        /// <summary>
        /// 新しいフレームを生成する。
        /// </summary>
        /// <param name="module">対象モジュール。</param>
        /// <param name="function">対象関数情報。</param>
        /// <param name="ip">このフレームの開始 IP（通常は function.CodeOffset）。</param>
        /// <param name="returnAddress">呼び出し元に戻る IP（エントリ関数の場合は -1）。</param>
        /// <param name="localCount">ローカル変数数。</param>
        public CallFrame(
            Module module,
            FunctionInfo function,
            int ip,
            int returnAddress,
            int localCount)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Function = function;
            Ip = ip;
            ReturnAddress = returnAddress;
            Locals = localCount > 0 ? new AloeValue[localCount] : Array.Empty<AloeValue>();
        }
    }
}
