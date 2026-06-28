using Azure.Storage.Queues.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace ProcessingFunctionApp;


public class QueueProcessorFunction
{
    private readonly ILogger<QueueProcessorFunction> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _configuration;
    private readonly string _containerName;
    private readonly string _dbName;

    public QueueProcessorFunction(ILogger<QueueProcessorFunction> logger, CosmosClient cosmosClient, IConfiguration configuration)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
        _configuration = configuration;
        _containerName = _configuration["CosmosDb:ContainerName"]!;
        _dbName = _configuration["CosmosDb:DatabaseName"]!;
    }

    [Function(nameof(QueueProcessorFunction))]
    public async Task Run([QueueTrigger("my-queue-name", Connection = "AzureQueue")] string messageId)
    {
        _logger.LogInformation("Processing queue message: {Message}", messageId);

        var container = _cosmosClient.GetContainer(_dbName, _containerName);

        var partitionKey = new PartitionKey(messageId);
        try
        {
            ItemResponse<MessageModel> response = await container.ReadItemAsync<MessageModel>(messageId, partitionKey);
            MessageModel existingItem = response.Resource;

            //mock processing
            await Task.Delay(Random.Shared.Next(1_000, 5_000));

            if (existingItem.ProcessedAt == null)
            {
                existingItem.ProcessedAt = DateTimeOffset.UtcNow;

                await container.ReplaceItemAsync(existingItem, messageId, partitionKey);
                _logger.LogInformation("Successfully updated item {Id} with current timestamp.", messageId);
            }
            else
            {
                _logger.LogError("Business rule violation: Item {Id} already contains a timestamp ({Timestamp}).", messageId, existingItem.ProcessedAt);
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError("Business rule violation: Object with ID {Id} does not exist in Cosmos DB.", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing item {Id}.", messageId);
            throw;
        }
    }
}