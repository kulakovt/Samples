using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeqEditor
{
    internal sealed class Story
    {
        public void Run()
        {
            var mainRepository = new DocumentRepository("The Outer Limits");
            var user1 = new Session("Acid Burn", mainRepository);
            var user2 = new Session("Zero Cool", mainRepository);

            var secondRepository = new DocumentRepository("Neuromancer");
            var friends = new[]
            {
                user1,
                user2,
                new Session("Phantom Phreak", secondRepository),
                new Session("Lord Nikon", secondRepository),
                new Session("Mr. The Plague", secondRepository),
                new Session("Cereal Killer", secondRepository),
                new Session("Crash Override", secondRepository)
            };

            //LongEdit(friends);

            ShortEdit(friends);

            user1.SaveDocument();
            user2.SaveDocument();
        }

        private static void LongEdit(Session[] friends)
        {
            var random = new Random();

            foreach (var friend in friends)
            {
                friend.Login();
                friend.OpenDocument();
            }

            while (true)
            {
                Parallel.ForEach(friends, friend =>
                {
                    var wait = TimeSpan.FromSeconds(random.Next(0, 60));
                    Thread.Sleep(wait);

                    switch (random.Next(0, 100))
                    {
                        case 0:
                            friend.MakeError();
                            break;
                        case 1:
                            friend.HackTheWorld();
                            break;
                        default:
                            friend.EditDocument();
                            break;
                    }
                });
            }
        }

        private static void ShortEdit(Session[] friends)
        {
            Parallel.ForEach(friends, friend =>
            {
                friend.Login();
                friend.OpenDocument();

                for (int i = 0; i < 50; i++)
                {
                    friend.EditDocument();
                }
            });
        }

        private static void OldLogger()
        {
            var logger = Program.BaseLogger.ForContext("Logger", "Old");

            logger.Information(
                "{0} is the {1} prime number. Its mirror, {2}, is the {3} and its mirror, {4}, is the product of multiplying {5}",
                73,
                "21st",
                37,
                "12th",
                21,
                "7 and 3"
                );

            const string userName = "People";
            logger.Warning("Hey {0}, stop using old logger!", userName);

        }

    }
}