var builder = DistributedApplication.CreateBuilder(args);



var cosmos = builder.AddAzureCosmosDB("cosmos")
                    .RunAsPreviewEmulator(c=>c.WithDataExplorer());

var db = cosmos.AddCosmosDatabase("ItemsDb");

var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator();
var queues = storage.AddQueues("AzureQueue");


builder.AddProject<Projects.AzureTestApp>("azuretestapp").WithReference(cosmos).WithReference(queues)
    .WaitFor(db);

builder.AddAzureFunctionsProject<Projects.ProcessingFunctionApp>("processingfunctionapp").WithReference(queues).WithReference(cosmos)
    .WaitFor(storage).WaitFor(db);

builder.Build().Run();
