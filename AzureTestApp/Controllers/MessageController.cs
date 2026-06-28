using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace AzureTestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<MessageController> _logger;
        private readonly Container _container;

        public MessageController(QueueClient queueClient, ILogger<MessageController> logger, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _queueClient = queueClient;
            _logger = logger;
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:ContainerName"];
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }
        [HttpPost]
        public async Task<IActionResult> AddMessageToSave(MessageDto messageObject)
        {
            var message = messageObject.Message;
            var messageId = Guid.NewGuid();

            var messageModel = new MessageModel
            {
                Id = messageId.ToString(),
                Data = message,
                ReceivedAt = DateTime.Now,
            };

            var alreadyExisting = _container.ReadItemAsync<MessageModel>(messageId.ToString(), new PartitionKey());

            try
            {
                _ = await _container.CreateItemAsync(messageModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving to CosmosDb");
                return BadRequest();
            }

            try
            {
                var result = await _queueClient.SendMessageAsync(messageModel.Id);
                return Accepted(value: messageModel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving to Azure Queue, trying to remove record from storage");
                _ = await _container.DeleteItemAsync<MessageModel>(messageId.ToString(), new PartitionKey(messageId.ToString()));


                return BadRequest();
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetState(string id)
        {
            var partitionKey = new PartitionKey(id);
            try
            {
                ItemResponse<MessageModel> response = await _container.ReadItemAsync<MessageModel>(id, partitionKey);
                MessageModel existingItem = response.Resource;

                //mock processing
                await Task.Delay(Random.Shared.Next(1_000, 5_000));

                if (existingItem.ProcessedAt == null)
                {
                    return Ok("Processing");
                }
                else
                {
                    return Ok("Processed");
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing item {Id}.", id);
                throw;
            }

        }

    }
}
