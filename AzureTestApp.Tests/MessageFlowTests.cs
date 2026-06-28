using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;

namespace AzureTestApp.Tests
{
    public sealed class MessageFlowTests : IClassFixture<AspireAppFixture>
    {
        private readonly HttpClient _client;

        public MessageFlowTests(AspireAppFixture fixture)
        {
            _client = fixture.ApiClient;
        }

        [Fact]
        public async Task PostMessage_EventuallyReturnsProcessed()
        {
            var postResponse = await _client.PostAsync(
                "/api/message",new StringContent("{\"message\":\"hello\"}",Encoding.UTF8,MediaTypeNames.Application.Json));

            postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var createdId = await postResponse.Content.ReadAsStringAsync();

            createdId.Should().NotBeNullOrWhiteSpace();

            string status = string.Empty;

            for (var i = 0; i < 20; i++)
            {
                var response = await _client.GetAsync($"/api/message/{createdId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                status = await response.Content.ReadAsStringAsync();

                if (status == "Processed")
                {
                    break;
                }

                await Task.Delay(1000);
            }

            status.Should().Be("Processed");
        }

        [Fact]
        public async Task GetMessage_Returns404_WhenMissing()
        {
            var response = await _client.GetAsync($"/message/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
