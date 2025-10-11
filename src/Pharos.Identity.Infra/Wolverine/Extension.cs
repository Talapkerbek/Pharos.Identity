using System.Reflection;
using JasperFx;
using JasperFx.Core;
using JasperFx.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Kafka;
using Wolverine.Postgresql;

namespace Pharos.Identity.Infra.Wolverine;


public static class Extensions
{
    public static ConfigureHostBuilder AddWolverineWithAssemblyDiscovery
    (
        this ConfigureHostBuilder host,
        IConfiguration configuration,
        List<Assembly> assemblies 
    )
    {
        
        host.UseWolverine(opts =>
        {
            // If we encounter a concurrency exception, just try it immediately
            // up to 3 times total
            opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);

            // It's an imperfect world, and sometimes transient connectivity errors
            // to the database happen
            opts.Policies.OnException<NpgsqlException>()
                .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    
            /*opts.UseKafka("")
                .ConfigureClient(client =>
                {
                    // configure both producers and consumers

                })
                .ConfigureConsumers(consumer =>
                {
                    // configure only consumers
                })

                .ConfigureProducers(producer =>
                {
                    // configure only producers
                })
            
                .ConfigureProducerBuilders(builder =>
                {
                    // there are some options that are only exposed
                    // on the ProducerBuilder
                })
            
                .ConfigureConsumerBuilders(builder =>
                {
                    // there are some Kafka client options that
                    // are only exposed from the builder
                })
            
                .ConfigureAdminClientBuilders(builder =>
                {
                    // configure admin client builders
                });*/
    
            opts.UseKafka("localhost:9094").AutoProvision();
            
            // opts.PublishMessage<UserCreatedEvent>().ToKafkaTopic("UserCreatedEvent");
            
            var connectionString = configuration.GetConnectionString("PostgresSQL");

            if (connectionString == null)
            {
                throw new NullReferenceException();
            }
            opts.PersistMessagesWithPostgresql(connectionString, "kafka");
            opts.UseEntityFrameworkCoreTransactions();

            foreach (var assembly in assemblies) 
            {
                opts.Discovery.IncludeAssembly(assembly);
            }
    
            // Adding outbox on all publish
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
            
            // Enrolling all local queues into the
            // durable inbox/outbox processing
            opts.Policies.UseDurableLocalQueues();
            
            // Adding inbox on all consumers
            opts.Policies.UseDurableInboxOnAllListeners();
            
            // Auto applying transactions, calls SaveChangesAsync auto-ly after handler ending 
            opts.Policies.AutoApplyTransactions();
            
            opts.UseFluentValidation();

            host.UseResourceSetupOnStartup();
        });
        
        return host;
    }
}
