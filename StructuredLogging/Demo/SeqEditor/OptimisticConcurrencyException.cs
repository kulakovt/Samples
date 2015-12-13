using System;

namespace SeqEditor
{
    internal class OptimisticConcurrencyException : Exception
    {
        public OptimisticConcurrencyException()
            : base("The document you attempted to edit was modified by another user after you got the original version")
        {
        }
    }
}