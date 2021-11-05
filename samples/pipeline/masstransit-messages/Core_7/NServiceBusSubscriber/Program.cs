using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;

namespace NServiceBusSubscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Console.Title = "NServiceBusSubscriber";

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services here
                })
                .UseNServiceBus(context =>
                {
                    var endpointConfiguration = new EndpointConfiguration("NServiceBusSubscriber");

                    endpointConfiguration.UsePersistence<InMemoryPersistence>();

                    #region Serializer
                    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                    #endregion

                    #region Transport
                    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                    transport.ConnectionString("host=hostos;username=rabbitmq;password=rabbitmq");
                    transport.UseConventionalRoutingTopology();
                    #endregion

                    #region Conventions
                    endpointConfiguration.Conventions()
                        .DefiningCommandsAs(type => type.Namespace?.EndsWith(".Commands") ?? false)
                        .DefiningEventsAs(type => type.Namespace?.EndsWith(".Events") ?? false)
                        .DefiningMessagesAs(type => type.Namespace?.EndsWith(".Messages") ?? false);
                    #endregion

                    #region RegisterBehavior
                    endpointConfiguration.Pipeline.Register(typeof(MassTransitIngestBehavior), "Ingests MassTransit messages.");
                    #endregion

                    endpointConfiguration.EnableInstallers();

                    return endpointConfiguration;
                });
        }
    }
}
