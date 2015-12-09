namespace SeqEditor
{
    internal sealed class DocumentRepository
    {
        private readonly Document documentStore;

        public DocumentRepository(string documentName)
        {
            documentStore = new Document(documentName);
        }

        public Document FindDocument()
        {
            return documentStore;
        }

        public void Save(Document document)
        {
            document.SaveCount++;

            if (document.SaveCount % 2 == 0)
            {
                throw new OptimisticConcurrencyException();
            }
        }
    }
}