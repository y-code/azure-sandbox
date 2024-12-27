using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ycode.Cosmos;
using Ycode.Storage;

namespace Ycode.Function;

public class HttpExample
{
    private readonly ILogger<HttpExample> _logger;
    private readonly StorageExample _storage;
    private readonly DataAccess _dataAccess;

    public HttpExample(ILogger<HttpExample> logger, StorageExample storage, DataAccess dataAccess)
    {
        _logger = logger;
        _storage = storage;
        _dataAccess = dataAccess;
    }

    [Function("HttpExample")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        await _storage.ProcessInStorageAsync();

        return new OkObjectResult($"Welcome to Azure Functions!");
    }

    [Function("ListBlobs")]
    public async Task<IActionResult> Run2([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        var containerName = req.Query["container"];
        if (string.IsNullOrEmpty(containerName))
            return new BadRequestObjectResult("'container' query is required.");

        var blobs = await _storage.ListBlobsAsync(containerName!);

        return new OkObjectResult($"The container '{containerName}' has the blobs:\n{string.Join("\n", blobs)}");
    }

    [Function("GetTextBlob")]
    public async Task<IActionResult> Run3([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var containerName = req.Query["container"];
        var blobName = req.Query["blob"];
        if (string.IsNullOrEmpty(containerName))
            return new BadRequestObjectResult("'container' query is required.");
        if (string.IsNullOrEmpty(blobName))
            return new BadRequestObjectResult("'blob' query is required.");

        var content = await _storage.GetBlobAsync(containerName!, blobName!);
        return new OkObjectResult(content);
    }

    [Function("Delete")]
    public async Task<IActionResult> Run4([HttpTrigger(AuthorizationLevel.Anonymous, "delete")] HttpRequest req)
    {
        var containerName = req.Query["container"];
        if (string.IsNullOrEmpty(containerName))
            return new BadRequestObjectResult("'container' query is required.");
        await _storage.DeleteContainerAsync(containerName!);
        return new OkObjectResult($"The '{containerName}' container has been deleted.");
    }

    [Function("InitDatabase")]
    public async Task<IActionResult> Run5([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        await _dataAccess.InitDatabaseAsync();

        return new OkObjectResult($"Have got connected to a database!");
    }

    [Function("CreateItem")]
    public async Task<IActionResult> Run6([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        await _dataAccess.CreateItemAsync();

        return new OkObjectResult($"Have created an item in the database!");
    }
}
