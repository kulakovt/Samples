using System;
using System.Threading;
using Serilog;

namespace SeqEditor
{
    internal sealed class Session
    {
        public readonly string UserName;
        public readonly DocumentRepository Repository;

        private readonly ILogger logger;
        private readonly string[] allChanges = Enum.GetNames(typeof(ChangeType));
        private static readonly Random Random = new Random();

        private Document document;

        public Session(string userName, DocumentRepository repository)
        {
            UserName = userName;
            Repository = repository;

            logger = Program.BaseLogger.ForContext("SessionId", UserName);
        }

        public void Login()
        {
            logger.Information("User {UserName} logged in", UserName);
        }

        public void OpenDocument()
        {
            logger.Information("Searching document...");
            document = Repository.FindDocument();

            logger.Information("Found document {Document}", document.Name);
            logger.Information("Open document {Document}", document.Name);
        }

        public void EditDocument()
        {
            var changeType = allChanges[Random.Next(0, allChanges.Length - 1)];

            var duration = TimeSpan.FromMilliseconds(Random.Next(100, 500));
            Thread.Sleep(duration);

            logger.Information("{Change} in document {Document} (take {Duration})", changeType, document.Name, duration);
        }

        public void SaveDocument()
        {
            logger.Information("Saving document {Document}...", document.Name);

            try
            {
                Repository.Save(document);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Save document {Document} failed", document.Name);
                return;
            }

            logger.Information("Document {Document} saved", document.Name);
        }

        public void HackTheWorld()
        {
            logger.Warning("Hack the world");
        }

        public void MakeError()
        {
            logger.Error("Buffer overflow");
        }

        private enum ChangeType
        {
            AddTitle,
            RemoveTitle,
            ChangeTitle,
            AddText,
            RemoveText,
            ChangeText,
            AddPicture,
            RemovePicture,
            ResizePicture,
            ChangeStyle,
        }
    }
}