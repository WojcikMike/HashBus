﻿namespace HashBus.Viewer.TopTweeters
{
    using ColoredConsole;
    using HashBus.WebApi;
    using RestSharp;

    static class TopTweetersLeaderBoardViewFactory
    {
        public static LeaderboardView<UserEntry> Create(
            string track,
            int refreshInterval,
            bool showPercentages,
            int verticalPadding,
            int horizontalPadding,
            IRestClient client)
        {
            return new LeaderboardView<UserEntry>(
                track,
                refreshInterval,
                new LeaderboardService<UserEntry>(client, "/top-tweeters/{0}"),
                showPercentages,
                verticalPadding,
                horizontalPadding,
                (entry1, entry2) => entry1.Id == entry2.Id,
                entry => new[] { $" {entry.Name}".White(), $" @{entry.ScreenName}".Cyan(), },
                "Top Tweeters",
                "tweets");
        }
    }
}
