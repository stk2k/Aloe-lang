using System;

namespace Aloe.CommonLib
{
    public class FunctionInfo
    {
        public string Name { get; }
        public int CodeOffset { get; }
        public int LocalCount { get; }
        public int ArgCount { get; }

        public FunctionInfo(string name, int codeOffset, int localCount, int argCount)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CodeOffset = codeOffset;
            LocalCount = localCount;
            ArgCount = argCount;
        }
    }
}
