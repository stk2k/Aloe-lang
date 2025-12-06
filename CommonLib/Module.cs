using Aloe.CommonLib.Exceptions;
using System;

namespace Aloe.CommonLib
{
    public class Module
    {
        public byte[] Code { get; }
        public IReadOnlyList<FunctionInfo> Functions { get; }
        public AloeValue[] Constants { get; }
        public int EntryPointIndex { get; }

        public Module(
            byte[] code,
            IList<FunctionInfo> functions,
            AloeValue[] constants,
            int entryPointIndex)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            if (functions == null) throw new ArgumentNullException(nameof(functions));
            if (constants == null) throw new ArgumentNullException(nameof(constants));

            Functions = new List<FunctionInfo>(functions);
            Constants = constants;

            if (entryPointIndex < 0 || entryPointIndex >= Functions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(entryPointIndex));
            }

            EntryPointIndex = entryPointIndex;
        }
    }
}
