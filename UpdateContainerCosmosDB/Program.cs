using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using UpdateContainerCosmosDB.Constants;

class Program
{
    private static CosmosClient? cosmosClient;

    static async Task Main(string[] args)
    {
        cosmosClient = new CosmosClient(CosmosDB.EndpointUri, CosmosDB.PrimaryKey);

        while (true) // Restart loop
        {
            Console.WriteLine("\n*** Cosmos DB Operations ***\n");

            try
            {
                var container = cosmosClient.GetContainer(CosmosDB.DatabaseName, CosmosDB.ContainerName);
                await ExecuteQueryAndOperations(container);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            Console.WriteLine("\nDo you want to restart the program? (Y/N)");
            var restartInput = Console.ReadLine()?.ToUpperInvariant();

            if (restartInput != "Y" && restartInput != "YES")
            {
                Console.WriteLine("Exiting the program...");
                break; // Exit the loop and terminate the program
            }
        }
    }

    private static async Task ExecuteQueryAndOperations(Container container)
    {
        var itemsToProcess = new List<Dictionary<string, object>>();
        bool isValidQuery = false;

        while (!isValidQuery)
        {
            try
            {
                Console.WriteLine("Enter SQL query to filter items (e.g., SELECT * FROM c WHERE c.id = '<id>'):");
                var sqlQuery = Console.ReadLine();

                itemsToProcess = await FetchItemsAsync(container, sqlQuery);
                if (itemsToProcess.Count == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("No items found. Try another query.");
                    continue; // Loop back to retry
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"Found {itemsToProcess.Count} item(s).");
                    isValidQuery = true;
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error querying Cosmos DB: {ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Invalid query. Error: {ex.Message}");
            }
        }

        await SelectOperation(container, itemsToProcess);
    }

    private static async Task<List<Dictionary<string, object>>> FetchItemsAsync(Container container, string query)
    {
        var items = new List<Dictionary<string, object>>();
        var queryDefinition = new QueryDefinition(query);

        using var feedIterator = container.GetItemQueryIterator<Dictionary<string, object>>(queryDefinition);
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync();
            items.AddRange(response);
        }

        foreach (var item in items)
        {
            Console.WriteLine();
            Console.WriteLine($"Item: {JsonConvert.SerializeObject(item)}");
        }

        return items;
    }

    private static async Task SelectOperation(Container container, List<Dictionary<string, object>> itemsToProcess)
    {
        Console.WriteLine("Choose operation: UPDATE or DELETE");
        string? operation = Console.ReadLine()?.ToUpperInvariant();

        switch (operation)
        {
            case "DELETE":
                await ConfirmAndDeleteItems(container, itemsToProcess);
                break;

            case "UPDATE":
                await ConfirmAndUpdateItems(container, itemsToProcess);
                break;

            default:
                Console.WriteLine("Invalid operation. Please try again.");
                await SelectOperation(container, itemsToProcess);
                break;
        }
    }

    private static async Task ConfirmAndDeleteItems(Container container, List<Dictionary<string, object>> itemsToProcess)
    {
        Console.WriteLine();
        Console.WriteLine("This operation will DELETE all selected items. Proceed? (Y/N)");
        var confirmation = Console.ReadLine()?.ToUpperInvariant();

        if (confirmation == "Y" || confirmation == "YES")
        {
            await DeleteRecordsAsync(itemsToProcess, container);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Operation canceled.");
        }
    }

    private static async Task ConfirmAndUpdateItems(Container container, List<Dictionary<string, object>> itemsToProcess)
    {
        Console.WriteLine("Properties in selected items:");
        string properties = string.Join(", ", itemsToProcess.First().Keys);
        Console.WriteLine($"Available properties: {properties}");

        Console.WriteLine("Enter the property to update:");
        string? propertyToUpdate = Console.ReadLine();

        bool propertyExists = itemsToProcess.Any(item => item.ContainsKey(propertyToUpdate));

        if (!propertyExists)
        {
            Console.WriteLine();
            Console.WriteLine($"Property '{propertyToUpdate}' not found in items. Please try Again.");
            await ConfirmAndUpdateItems(container, itemsToProcess).ConfigureAwait(false);
        }
        else
        {
            Console.WriteLine("Enter the new value for the property:");
            string? newValue = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("This operation will UPDATE all selected items. Proceed? (Y/N)");
            string? confirmation = Console.ReadLine()?.ToUpperInvariant();

            if (confirmation == "Y" || confirmation == "YES")
            {
                KeyValuePair<string, object> updateParameter = new KeyValuePair<string, object>(propertyToUpdate, newValue);
                await UpsertRecordsAsync(itemsToProcess, container, updateParameter);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Operation canceled.");
            }
        }
    }

    public static async Task DeleteRecordsAsync(List<Dictionary<string, object>> itemsToProcess, Container container)
    {
        foreach (var item in itemsToProcess)
        {
            try
            {
                await container.DeleteItemAsync<Dictionary<string, object>>(
                    item["id"].ToString(),
                    new PartitionKey(item["id"].ToString())
                );
                Console.WriteLine();
                Console.WriteLine($"Deleted item with id: {item["id"]}");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Failed to delete item with id: {item["id"]}. Error: {ex.StatusCode} - {ex.Message}");
            }
        }
    }

    public static async Task UpsertRecordsAsync(List<Dictionary<string, object>> itemsToProcess, Container container, KeyValuePair<string, object> parameter)
    {
        foreach (var item in itemsToProcess)
        {
            try
            {
                item[parameter.Key] = parameter.Value;
                await container.UpsertItemAsync(item, new PartitionKey(item["id"].ToString()));
                Console.WriteLine();
                Console.WriteLine($"Updated item with id: {item["id"]}");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Failed to update item with id: {item["id"]}. Error: {ex.StatusCode} - {ex.Message}");
            }
        }
    }
}
