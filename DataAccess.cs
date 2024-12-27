using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ycode.Cosmos;

public class DataAccess(ILogger<DataAccess> logger, IConfiguration config)
{
    private const string ConfigCosmosAccountUri = "AZURE_COSMOS_ACCOUNT";
	private const string ConfigPrimaryKey = "AZURE_COSMOS_PRIMARY_KEY";

    private readonly Lazy<string> _cosmosAccountUri = new(() => {
        var uri = config.GetSection(ConfigCosmosAccountUri).Value;
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentException($"The environment variable {ConfigPrimaryKey} was not provided.");
        return uri;
    });
	private readonly Lazy<string> _cosmosPrimaryKey = new(() => {
        var key = config.GetSection(ConfigPrimaryKey).Value;
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"The environment variable {ConfigPrimaryKey} was not provided.");
        return key;
    });

	private const string __databaseId = "az204Database";
	private const string __containerId = "az204Container";

	public async Task InitDatabaseAsync()
	{
		try
		{
			var client = new CosmosClient(_cosmosAccountUri.Value, _cosmosPrimaryKey.Value);

			var dbRes = await client.CreateDatabaseIfNotExistsAsync(__databaseId);
			logger.LogInformation("Found database {Database}.", dbRes.Database.Id);

			var containerRes = await dbRes.Database.CreateContainerIfNotExistsAsync(__containerId, "/LastName");
			logger.LogInformation("Found Container: {Container}.", containerRes.Container.Id);

			await containerRes.Container.Scripts.CreateTriggerAsync(new TriggerProperties
			{
				Id = "appendCreateedTime",
				Body = @"
function validateToDoItemTimestamp() {
    var context = getContext();
    var request = context.getRequest();

    // item to be created in the current operation
    var itemToCreate = request.getBody();

    // validate properties
    if (!('timestamp' in itemToCreate)) {
        var ts = new Date();
        itemToCreate['timestamp'] = ts.getTime();
    }

    // update the item that will be created
    request.setBody(itemToCreate);
}",
				TriggerOperation = TriggerOperation.Create,
				TriggerType = TriggerType.Pre,
			});

			var item = new ContactEntity
			{
				id = Guid.NewGuid().ToString(),
				LastName = "Baker",
				FirstName = "Ted",
				Relation = "customer",
				Mobile = "",
			};
			await containerRes.Container.CreateItemAsync(item);
		}
		catch (CosmosException ex)
		{
			logger.LogError(ex, "Something went wrong with Cosmos Database.");
		}
	}

	public async Task CreateItemAsync()
	{
		try
		{
			var client = new CosmosClient(_cosmosAccountUri.Value, _cosmosPrimaryKey.Value);

			var db = client.GetDatabase(__databaseId);
			logger.LogInformation("Found database {Database}.", db.Id);

			var container = db.GetContainer(__containerId);
			logger.LogInformation("Found Container: {Container}.", container.Id);

			var item = new ContactEntity
			{
				id = Guid.NewGuid().ToString(),
				LastName = "Baker",
				FirstName = "Ted",
				Relation = "customer",
				Mobile = "",
			};
			await container.CreateItemAsync(item, requestOptions: new ItemRequestOptions
			{
				PreTriggers = [ "appendCreateedTime" ],
			});
		}
		catch (CosmosException ex)
		{
			logger.LogError(ex, "Something went wrong with Cosmos Database.");
		}
	}
}