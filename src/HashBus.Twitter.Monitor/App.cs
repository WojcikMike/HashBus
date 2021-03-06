namespace HashBus.Twitter.Monitor
{
    using System.Threading.Tasks;
    using HashBus.NServiceBusConfiguration;
    using NServiceBus;
    using NServiceBus.Persistence;

    class App
    {
        public static async Task RunAsync(
            string nserviceBusConnectionString,
            string track,
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret,
            string endpointName)
        {
            var busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName(endpointName);
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UsePersistence<NHibernatePersistence>().ConnectionString(nserviceBusConnectionString);
            busConfiguration.ApplyMessageConventions();

            using (var bus = Bus.Create(busConfiguration).Start())
            {
                await Monitoring.StartAsync(
                    bus,
                    track,
                    consumerKey,
                    consumerSecret,
                    accessToken,
                    accessTokenSecret);
            }
        }
    }
}
