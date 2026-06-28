using Aspire.Hosting;
using Projects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureTestApp.Tests
{
    public sealed class AspireAppFixture : IAsyncLifetime
    {
        private DistributedApplication? _app;

        public HttpClient ApiClient { get; private set; } = default!;

        public async ValueTask InitializeAsync()
        {
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<AzureTestApp_AppHost>();

            _app = await builder.BuildAsync();

            await _app.StartAsync();

            ApiClient = _app.CreateHttpClient("azuretestapp");
        }

        public async ValueTask DisposeAsync()
        {
            ApiClient.Dispose();

            if (_app is not null)
            {
                await _app.DisposeAsync();
            }
        }

    }
}
