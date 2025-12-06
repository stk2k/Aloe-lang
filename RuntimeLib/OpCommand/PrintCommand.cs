// Aloe.Runtime/Execution/NopCommand.cs
using Aloe.CommonLib;
using Aloe.CommonLib.Constants;
using Aloe.CommonLib.Exceptions;
using Aloe.RuntimeLib;
using Microsoft.VisualBasic;
using System;

namespace Aloe.RuntimeLib.OpCommand
{
    internal sealed class PrintCommand : IOpcodeCommand
    {
        public void Execute(AloeVm vm, BytecodeReader reader)
        {
            var stack = vm.OperandStack;

            if (stack.Count == 0)
            {
                throw new VmException("Print requires at least one value on the stack.");
            }

            var value = stack.Pop();

            string text;

            switch (value.Kind)
            {
                case EnumValueKind.Int:
                    text = value.AsInt().ToString();
                    break;

                case EnumValueKind.Bool:
                    text = value.AsBool() ? "true" : "false";
                    break;

                case EnumValueKind.String:
                    text = value.AsString();
                    break;

                default:
                    // 必要に応じて AloeValue.ToString() の実装を充実させてもよい
                    text = value.ToString();
                    break;
            }

            Console.WriteLine(text);
        }
    }
}
