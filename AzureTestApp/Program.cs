
using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
namespace AzureTestApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            builder.AddAzureQueueServiceClient("AzureQueue", configureClientBuilder: builder => { builder.ConfigureOptions(x => x.MessageEncoding = QueueMessageEncoding.Base64); });
            builder.Services.AddSingleton<QueueClient>(x => x.GetRequiredService<QueueServiceClient>().GetQueueClient("my-queue-name"));






            // Configure Cosmos DB Client
            var cosmosSection = builder.Configuration.GetSection("CosmosDb");
            var databaseName = cosmosSection["DatabaseName"]!;
            var containerName = cosmosSection["ContainerName"];


            builder.AddAzureCosmosClient("cosmos", configureClientOptions: x => { x.UseSystemTextJsonSerializerWithOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web); });


            var app = builder.Build();
            var database = await app.Services.GetRequiredService<CosmosClient>().CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
            await app.Services.GetRequiredService<QueueClient>().CreateIfNotExistsAsync();
            app.UseSwagger();
            app.UseSwaggerUI();


            app.MapDefaultEndpoints();
            app.MapSwagger();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
