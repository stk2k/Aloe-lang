using Aloe.CommonLib;
using Aloe.RuntimeLib;
using System;
using System.Collections.Generic;

namespace AloeSample
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            // 1. 定数テーブルを構築
            //    0: int 0
            //    1: int 5
            //    2: int 1
            //    3: string "Done"
            var constants = new List<AloeValue>
            {
                AloeValue.FromInt(0),          // const[0]
                AloeValue.FromInt(5),          // const[1]
                AloeValue.FromInt(1),          // const[2]
                AloeValue.FromString("Done"),  // const[3]
            };

            // 2. バイトコードを構築
            //    疑似コード:
            //
            //    int i = 0;
            //    while (i < 5)
            //    {
            //        print(i);
            //        i = i + 1;
            //    }
            //    print("Done");
            //
            var builder = new BytecodeBuilder();

            // ローカル変数:
            //   local[0] = i

            // i = 0;
            builder.Emit(Opcode.PushConst);   // const[0] = 0
            builder.EmitInt32(0);
            builder.Emit(Opcode.StoreLocal);
            builder.EmitInt32(0);

            // loop_start:
            builder.MarkLabel("loop_start");

            // if (!(i < 5)) break;
            builder.Emit(Opcode.LoadLocal);   // i
            builder.EmitInt32(0);
            builder.Emit(Opcode.PushConst);   // const[1] = 5
            builder.EmitInt32(1);
            builder.Emit(Opcode.CmpLt);       // i < 5
            builder.EmitJump(Opcode.JumpIfFalse, "loop_end");

            // print(i);
            builder.Emit(Opcode.LoadLocal);
            builder.EmitInt32(0);
            builder.Emit(Opcode.Print);

            // i = i + 1;
            builder.Emit(Opcode.LoadLocal);
            builder.EmitInt32(0);
            builder.Emit(Opcode.PushConst);   // const[2] = 1
            builder.EmitInt32(2);
            builder.Emit(Opcode.Add);
            builder.Emit(Opcode.StoreLocal);
            builder.EmitInt32(0);

            // jump loop_start;
            builder.EmitJump(Opcode.Jump, "loop_start");

            // loop_end:
            builder.MarkLabel("loop_end");

            // print("Done");
            builder.Emit(Opcode.PushConst);   // const[3] = "Done"
            builder.EmitInt32(3);
            builder.Emit(Opcode.Print);

            // 終了
            builder.Emit(Opcode.Halt);

            // ラベルのジャンプオフセットを解決
            var code = builder.ToArray();

            // 3. 関数テーブルを構築
            // main 関数だけを持つシンプルなモジュール
            var functions = new List<FunctionInfo>
            {
                new FunctionInfo(
                    name: "main",
                    entryIp: 0,
                    localCount: 1  // local[0] = i
                )
            };

            // 4. モジュールを作成
            var module = new Module(
                code: code,
                functions: functions,
                constants: constants,
                entryFunctionIndex: 0
            );

            // 5. VM を起動して実行
            var vm = new AloeVm(module);
            vm.Run();

            // 実行結果:
            // 0
            // 1
            // 2
            // 3
            // 4
            // Done
        }
    }

    /// <summary>
    /// バイトコード構築用の簡易ビルダ。
    /// ラベルを使って Jump / JumpIfFalse のオフセットを後から解決する。
    /// </summary>
    internal sealed class BytecodeBuilder
    {
        private readonly List<byte> _code = new();
        private readonly Dictionary<string, int> _labels = new();
        private readonly List<PatchTarget> _patches = new();

        private struct PatchTarget
        {
            public int Position;      // オフセットを書き込む位置（int32 の先頭インデックス）
            public string LabelName;  // ジャンプ先ラベル名
        }

        public int Position => _code.Count;

        public void Emit(Opcode opcode)
        {
            _code.Add((byte)opcode);
        }

        public void EmitInt32(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            _code.AddRange(bytes);
        }

        /// <summary>
        /// Jump / JumpIfFalse などの「相対オフセットを取る命令」をエミットする。
        /// オフセットは後でラベル解決時にパッチされる。
        /// </summary>
        public void EmitJump(Opcode opcode, string labelName)
        {
            Emit(opcode);

            // オフセットを書き込む位置を覚えておき、いったん 0 を入れておく
            int patchPos = Position;
            EmitInt32(0);

            _patches.Add(new PatchTarget
            {
                Position = patchPos,
                LabelName = labelName,
            });
        }

        public void MarkLabel(string name)
        {
            if (_labels.ContainsKey(name))
            {
                throw new InvalidOperationException($"Label '{name}' is already defined.");
            }
            _labels[name] = Position;
        }

        public byte[] ToArray()
        {
            FixupJumps();
            return _code.ToArray();
        }

        private void FixupJumps()
        {
            foreach (var patch in _patches)
            {
                if (!_labels.TryGetValue(patch.LabelName, out var targetPos))
                {
                    throw new InvalidOperationException($"Label '{patch.LabelName}' is not defined.");
                }

                // patch.Position は「int32 オフセットを書き込む先頭バイトのインデックス」
                // 命令の仕様:
                //   IP は命令バイトの次の位置を指している状態で ReadInt32 され、
                //   Jump / JumpIfFalse 側では「(Read 直後の IP) + offset」がジャンプ先になる。
                //
                // よってオフセットは:
                //   targetPos - (afterImmediatePos)
                // として求める。
                int afterImmediatePos = patch.Position + 4;
                int offset = targetPos - afterImmediatePos;

                var bytes = BitConverter.GetBytes(offset);
                for (int i = 0; i < 4; i++)
                {
                    _code[patch.Position + i] = bytes[i];
                }
            }
        }
    }
}
