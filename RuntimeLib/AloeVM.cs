using Aloe.CommonLib;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib.OpCommand;
using System;
using System.Collections.Generic;

namespace Aloe.RuntimeLib
{
    /// <summary>
    /// AloeVM 本体。各命令の実装は Command パターンで分離される。
    /// </summary>
    public class AloeVm
    {
        private Module? _module;
        private BytecodeReader? _reader;
        private readonly OperandStack _operandStack = new OperandStack();
        private readonly CallStack _callStack = new CallStack();

        private readonly Dictionary<Opcode, IOpcodeCommand> _commands;

        /// <summary>
        /// VM のコンストラクタ。
        /// ★ ここは「戻り値型を付けない」のがポイント（void などを書かない）
        /// </summary>
        public AloeVm()
        {
            _commands = BuildCommandTable();
        }

        /// <summary>
        /// 指定されたモジュールを実行し、終了コードを返す。
        /// </summary>
        public int Run(Module module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            _module = module;
            _reader = new BytecodeReader(module.Code);
            _operandStack.Clear();
            _callStack.Clear();

            // エントリ関数フレームを作成
            var entry = module.Functions[module.EntryPointIndex];
            var frame = new CallFrame(
                module: module,
                function: entry,
                ip: entry.CodeOffset,
                returnAddress: -1,
                localCount: entry.LocalCount
            );
            _callStack.Push(frame);

            _reader.Position = entry.CodeOffset;

            try
            {
                while (true)
                {
                    if (_reader.Position >= module.Code.Length)
                    {
                        throw new VmException("Unexpected end of bytecode.");
                    }

                    var op = (Opcode)_reader.ReadByte();

                    if (!_commands.TryGetValue(op, out var command))
                    {
                        throw new VmException($"Unknown opcode: {op}");
                    }

                    command.Execute(this, _reader);
                }
            }
            catch (VmHaltException ex)
            {
                // Halt または エントリ関数 Ret による正常終了
                return ex.ExitCode;
            }
        }

        //==============================
        // VM 内部状態へのアクセサ
        //==============================

        internal Module Module
            => _module ?? throw new InvalidOperationException("VM is not running (Module is null).");

        internal BytecodeReader Reader
            => _reader ?? throw new InvalidOperationException("VM is not running (Reader is null).");

        internal OperandStack OperandStack => _operandStack;

        internal CallStack CallStack => _callStack;

        /// <summary>
        /// 現在のフレーム。
        /// ref 戻り値は使わず、struct のコピーで十分（Locals は参照型）。
        /// </summary>
        internal CallFrame CurrentFrame => _callStack.Peek();

        //==============================
        // コマンドテーブル構築
        //==============================

        private Dictionary<Opcode, IOpcodeCommand> BuildCommandTable()
        {
            return new Dictionary<Opcode, IOpcodeCommand>
            {
                { Opcode.Nop,         new NopCommand() },
                { Opcode.Halt,        new HaltCommand() },

                { Opcode.PushConst,   new PushConstCommand() },
                { Opcode.LoadLocal,   new LoadLocalCommand() },
                { Opcode.StoreLocal,  new StoreLocalCommand() },

                { Opcode.Add,         new AddCommand() },
                { Opcode.Sub,         new SubCommand() },
                { Opcode.Mul,         new MulCommand() },
                { Opcode.Div,         new DivCommand() },
                { Opcode.CmpLt,       new CmpLtCommand() },

                { Opcode.Jump,        new JumpCommand() },
                { Opcode.JumpIfFalse, new JumpIfFalseCommand() },

                { Opcode.Print,       new PrintCommand() },

                { Opcode.Call,        new CallCommand() },
                { Opcode.Ret,         new RetCommand() },
            };
        }

        /// <summary>
        /// VM を正常終了させるヘルパー。
        /// Halt 命令やエントリ関数の Ret から呼び出されることを想定。
        /// </summary>
        /// <param name="exitCode">終了コード。</param>
        internal void Halt(int exitCode)
        {
            throw new VmHaltException(exitCode);
        }

        /// <summary>
        /// 終了コード 0 で VM を終了させるショートカット。
        /// </summary>
        internal void Halt()
        {
            Halt(0);
        }
    }

    /// <summary>
    /// VM の正常終了を表す内部用例外。
    /// Halt 命令や、エントリ関数の Ret からスローされる。
    /// </summary>
    public sealed class VmHaltException : Exception
    {
        public int ExitCode { get; }

        public VmHaltException(int exitCode)
            : base($"VM halted with exit code {exitCode}.")
        {
            ExitCode = exitCode;
        }
    }
}
