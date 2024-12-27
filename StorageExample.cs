using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ycode.Storage;

public class StorageExample(ILogger<StorageExample> logger, IConfiguration config)
{
    private const string ConfigStorageAccountName = "AZURE_STORAGE_ACCOUNT_NAME";
    private const string ConfigStorageAccountKey = "AZURE_STORAGE_ACCOUNT_KEY";

    private Lazy<string> _storageConnStr = new(() => {
        var name = config.GetSection(ConfigStorageAccountName).Value;
        var key = config.GetSection(ConfigStorageAccountKey).Value;
        return $"DefaultEndpointsProtocol=https;AccountName={name};AccountKey={key};EndpointSuffix=core.windows.net";
    });

    public async Task ProcessInStorageAsync()
    {
        var client = new BlobServiceClient(_storageConnStr.Value);

        var containerName = $"wtblob{Guid.NewGuid()}";
        var containerClient = await client.CreateBlobContainerAsync(containerName);

        logger.LogInformation("A container named '{ContainerName}' has been created", containerName);

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteLineAsync("Hello, World!");
        writer.Flush();
        stream.Position = 0;

        var fileName = $"wtfile{Guid.NewGuid()}.txt";
        var blobClient = containerClient.Value.GetBlobClient(fileName);

        logger.LogInformation("Uploading a blob to blob storage: {URU}", blobClient.Uri);
        await blobClient.UploadAsync(stream);
        logger.LogInformation("Uploaded a blob to blob storage: {URI}", blobClient.Uri);
    }

    public async Task<IReadOnlyList<string>> ListBlobsAsync(string containerName)
    {
        var client = new BlobServiceClient(_storageConnStr.Value);
        var containerClient = client.GetBlobContainerClient(containerName);
        var blobNames = new List<string>();
        await foreach (var blob in containerClient.GetBlobsAsync())
        {
            blobNames.Add(blob.Name);
        }
        return blobNames.AsReadOnly();
    }

    public async Task<string> GetBlobAsync(string containerName, string blobName)
    {
        var client = new BlobServiceClient(_storageConnStr.Value);
        var containerClient = client.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        if (!(await blobClient.ExistsAsync()).Value)
            throw new InvalidOperationException($"The blob /{containerName}/{blobName} was not found.");
        
        var stream = await blobClient.DownloadStreamingAsync();
        using var reader = new StreamReader(stream.Value.Content);
        return await reader.ReadToEndAsync();
    }

    public async Task DeleteContainerAsync(string containerName)
    {
        var client = new BlobServiceClient(_storageConnStr.Value);
        var containerClient = client.GetBlobContainerClient(containerName);
        if (!(await containerClient.ExistsAsync()).Value)
            throw new InvalidOperationException($"The container '{containerName}' was not found.");

        await containerClient.DeleteAsync();
    }
}
