using System;
using System.Collections.Generic;
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.RuntimeLib;

namespace AloeSample
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // 1. 定数プール
            var constants = new List<AloeValue>
            {
                AloeValue.FromString("Hello, world")
            };

            // 2. 命令列: PushConst(0); Syscall(Print); Halt
            var code = new List<Instruction>
            {
                // const[0] ("Hello, world") をスタックに積む
                new Instruction(EnumOpcode.PushConst, 0),

                // Syscall(Print) を呼ぶ
                new Instruction(EnumOpcode.Syscall, (int)EnumSyscallId.Print),

                // プログラム終了
                new Instruction(EnumOpcode.Halt)
            };

            // 3. 関数情報 (main)
            var functions = new List<FunctionInfo>
            {
                new FunctionInfo(
                    name: "main",
                    entryIp: 0,   // code[0] から
                    parameterCount: 0,
                    localCount: 0
                )
            };

            // 4. モジュール作成 (エントリポイントは main(= index 0))
            //   Module のコンストラクタが
            //   (constants, code, functions, entryPointIndex)
            //   の順になっている前提
            var module = new Module(
                constants,
                code,
                functions,
                entryPointIndex: 0
            );

            // 5. VM 作成（内部でオペコードディスパッチテーブルを構築）
            var vm = new AloeVm(module);

            // 6. print システムコール登録
            // AloeVm に RegisterSyscall(SyscallId, Action<AloeVm>) がある前提
            vm.RegisterSyscall(EnumSyscallId.Print, v =>
            {
                // スタックトップの値を取り出してコンソールに出力
                var value = v.Pop();        // AloeVm に Pop() がある前提
                Console.WriteLine(value);   // AloeValue.ToString() 経由
            });

            // 7. 実行
            vm.RunFromEntryPoint();
        }
    }
}
