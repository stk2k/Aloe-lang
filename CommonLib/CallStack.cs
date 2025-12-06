using System;

namespace Aloe.CommonLib
{
    public class CallStack
    {
        private readonly Stack<CallFrame> _frames = new Stack<CallFrame>();

        public int Count => _frames.Count;

        public void Push(CallFrame frame)
        {
            _frames.Push(frame);
        }

        public CallFrame Pop()
        {
            return _frames.Pop();
        }

        public CallFrame Peek()
        {
            return _frames.Peek();
        }

        public void Clear()
        {
            _frames.Clear();
        }
    }
}
