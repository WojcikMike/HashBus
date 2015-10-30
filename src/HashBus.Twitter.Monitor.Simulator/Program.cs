using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HashBus.Twitter.Events;
using NServiceBus;
using System.Linq;

namespace HashBus.Twitter.Monitor.Simulator
{
    class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            var busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName("HashBus.Twitter.Monitor");
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.SendFailedMessagesTo("error");

            using (var bus = await Bus.Create(busConfiguration).StartAsync())
            {
                await SimulateTwitter(bus);
            }
        }

        static async Task SimulateTwitter(ISendOnlyBus bus)
        {
            var random = new Random();
            var countOfUsers = 15;
            while (true)
            {
                Thread.Sleep((int)Math.Pow(random.Next(6), 5));
                var now = DateTime.UtcNow;
                var hashtag = "Simulated";
                if (now.Millisecond % 3 == 0)
                {
                    hashtag = hashtag.ToLowerInvariant();
                }

                var userId = random.Next(countOfUsers);
                var userMentionId = random.Next(countOfUsers);
                var userMentionIndex = random.Next(31) + 1;
                var retweetedUserId = random.Next(countOfUsers);
                var text = string.Join(
                        string.Empty,
                        Enumerable.Range(0, userMentionIndex - 1).Select(i => char.ConvertFromUtf32(random.Next(65, 128)))) +
                    $" @johnsmith{userMentionId} " +
                    string.Join(
                        string.Empty,
                        Enumerable.Range(0, random.Next(32)).Select(i => char.ConvertFromUtf32(random.Next(65, 128)))) +
                    $" #{hashtag}";

                var message = new HashtagTweeted
                {
                    Hashtag = hashtag,
                    TweetId = now.Ticks,
                    TweetCreatedAt = now,
                    TweetCreatedById = userId,
                    TweetCreatedByIdStr = $"{userId}",
                    TweetCreatedByName = $"John Smith{userId}",
                    TweetCreatedByScreenName = $"johnsmith{userId}",
                    TweetIsRetweet = now.Millisecond % 3 == 0,
                    TweetText = text,
                    TweetUserMentions = new List<UserMention>
                    {
                        new UserMention
                        {
                            Id=userMentionId,
                            IdStr= $"{userMentionId}",
                            Indices = new List<int> { userMentionIndex, userMentionIndex + $"@johnsmith{userMentionId}".Length, },
                            Name = $"John Smith{userMentionId}",
                            ScreenName = $"johnsmith{userMentionId}",
                        },
                    },
                    TweetHashtags = new List<Hashtag>
                    {
                        new Hashtag
                        {
                            Text = hashtag,
                            Indices = new[] { text.Length - $"#{hashtag}".Length, text.Length, },
                        },
                    },
                    RetweetedTweetId = now.AddDays(-1000).Ticks,
                    RetweetedTweetCreatedAt = now.AddDays(-1000),
                    RetweetedTweetCreatedById = retweetedUserId,
                    RetweetedTweetCreatedByIdStr = $"{retweetedUserId}",
                    RetweetedTweetCreatedByName = $"John Smith{retweetedUserId}",
                    RetweetedTweetCreatedByScreenName = $"johnsmith{retweetedUserId}",
                };

                Writer.Write(message);
                await bus.PublishAsync(message);
            }
        }
    }
}