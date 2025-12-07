using System.Collections.Generic;
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;

namespace AloeSample.Programs
{
    /// <summary>
    /// Main() から Add(a:int, b:int) を呼び出して 2 + 3 を計算するサンプル。
    ///
    /// 関数インデックス:
    ///   0: Main
    ///   1: Add
    ///
    /// コード配置:
    ///   Main.EntryIp = 0
    ///   Add.EntryIp  = 4
    /// </summary>
    public sealed class AddSampleProgram : ISampleProgram
    {
        /// <summary>
        /// 定数プール:
        ///   Const[0] = 2
        ///   Const[1] = 3
        /// </summary>
        public List<AloeValue> Constants { get; } = new()
        {
            AloeValue.FromInt(2),
            AloeValue.FromInt(3),
        };

        public List<Instruction> Code { get; } = new()
        {
            // main のエントリ: 0
            new Instruction(EnumOpcode.PushConst, 0, 0),                         // const[0] → 2
            new Instruction(EnumOpcode.PushConst, 1, 0),                         // const[1] → 3
            new Instruction(EnumOpcode.Add,       0, 0),                         // 2 + 3 = 5
            new Instruction(EnumOpcode.Syscall,   (int)EnumSyscall.Print, 0),    // Print(5)
            new Instruction(EnumOpcode.Halt,      0, 0),
        };

        public List<FunctionInfo> Functions { get; } = new()
        {
            // 関数 0: Main()
            new FunctionInfo(
                name: "Main",
                entryIp: 0,
                parameterCount: 0,
                localCount: 0  // Main は引数なし・ローカルなし
            ),

            // 関数 1: Add(a:int, b:int):int
            new FunctionInfo(
                name: "Add",
                entryIp: 4,
                parameterCount: 2, // a, b
                localCount: 2      // a, b をそのまま Locals[0,1] に置く
            ),
        };

        /// <summary>
        /// 関数インデックス定数。
        /// </summary>
        public static class FuncIndex
        {
            public const int Main = 0;
            public const int Add = 1;
        }
    }
}
