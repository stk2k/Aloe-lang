using System;

namespace Aloe.CommonLib
{
    /// <summary>
    /// VM から呼び出すシステムコール ID 一覧。
    /// </summary>
    public enum SyscallId : int
    {
        /// <summary>
        /// 標準出力に文字列を書き出す。
        /// args[0]: string
        /// </summary>
        WriteStdout = 1,

        // 将来拡張:
        // WriteStderr = 2,
        // GetTime     = 3,
    }
}
