namespace SeqEditor
{
    internal sealed class Document
    {
        public readonly string Name;
        public int SaveCount;

        public Document(string name)
        {
            Name = name;
        }
    }
}