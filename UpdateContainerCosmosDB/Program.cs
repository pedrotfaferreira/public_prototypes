using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UpdateContainerCosmosDB.Constants;
using UpdateContainerCosmosDB.Models;

class Program
{
    private static CosmosClient? cosmosClient;

    static async Task Main(string[] args)
    {
        cosmosClient = new CosmosClient(CosmosDB.EndpointUri, CosmosDB.PrimaryKey, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
        });

        while (true) // Restart loop
        {
            Console.WriteLine("\n*** Cosmos DB Operations ***\n");

            try
            {
                Console.WriteLine("Enter the Container you want to query: ");
                string containerName = Console.ReadLine();

                var container = cosmosClient.GetContainer(CosmosDB.DatabaseName, containerName);

                if (containerName == "UsageRecords")
                {
                    await ExecuteQueryAndOperations<UsageRecord>(container);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"This Cosmos DataBase doesn't have the container {containerName}. Want to try again? (Y/N)\"");
                    string? confirmation = Console.ReadLine()?.ToUpperInvariant();

                    if (confirmation == "Y" || confirmation == "YES")
                    {

                        continue; // Restart the loop
                    }
                    else
                    {
                        Console.WriteLine("Exiting the program...");
                        break; // Exit the loop and terminate the program
                    }
                }
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

    private static async Task ExecuteQueryAndOperations<T>(Container container)
    {
        var itemsToProcess = new List<T>();
        bool isValidQuery = false;

        while (!isValidQuery)
        {
            try
            {
                Console.WriteLine("Enter SQL query to filter items (e.g., SELECT * FROM c WHERE c.id = '<id>'):");
                var sqlQuery = Console.ReadLine();

                itemsToProcess = await FetchItemsAsync<T>(container, sqlQuery);
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
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Invalid query. Error: {ex.Message}");
            }
        }

        await SelectOperation(container, itemsToProcess);
    }

    private static async Task<List<T>> FetchItemsAsync<T>(Container container, string query)
    {
        var items = new List<T>();
        var queryDefinition = new QueryDefinition(query);

        using var feedIterator = container.GetItemQueryIterator<T>(queryDefinition);

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

    private static async Task SelectOperation<T>(Container container, List<T> itemsToProcess)
    {
        Console.WriteLine("Choose operation: UPDATE (new properties and values) or DELETE (records)");
        string? operation = Console.ReadLine()?.ToUpperInvariant();

        switch (operation)
        {


            case "UPDATE":
                await ConfirmAndUpdateItems(container, itemsToProcess);
                break;

            case "DELETE":
                await ConfirmAndDeleteItems(container, itemsToProcess);
                break;

            default:
                Console.WriteLine();
                Console.WriteLine("Invalid operation. Please try again.");
                await SelectOperation(container, itemsToProcess);
                break;
        }
    }

    private static async Task ConfirmAndDeleteItems<T>(Container container, List<T> itemsToProcess)
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

    private static async Task ConfirmAndUpdateItems<T>(Container container, List<T> itemsToProcess)
    {
        var properties = typeof(T).GetProperties().Select(p => p.Name);

        Console.WriteLine();
        Console.WriteLine($"Available properties: {string.Join(", ", typeof(T).GetProperties().Select(p => p.Name))}");
        Console.WriteLine();
        Console.WriteLine("Enter the property to update:");
        string? propertyToUpdate = Console.ReadLine();

        bool propertyExists = properties.Contains(propertyToUpdate);

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
                var propertyInfo = typeof(T).GetProperty(propertyToUpdate);
                if (propertyInfo != null)
                {
                    try
                    {
                        foreach (var item in itemsToProcess)
                        {

                            propertyInfo.SetValue(item, Convert.ChangeType(newValue, propertyInfo.PropertyType));
                            await UpsertRecordsAsync(itemsToProcess, container);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Failed to update item. Error: {ex.Message}. Please try Again.");
                        Console.WriteLine();
                        await ConfirmAndUpdateItems(container, itemsToProcess).ConfigureAwait(false);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Operation canceled.");
                }
            }
        }
    }

    public static async Task DeleteRecordsAsync<T>(List<T> itemsToProcess, Container container)
    {
        foreach (var item in itemsToProcess)
        {
            var propertyInfo = typeof(T).GetProperty("Id");
            string idValue = propertyInfo?.GetValue(item)?.ToString();

            try
            {
                await container.DeleteItemAsync<T>(idValue, new PartitionKey(idValue));

                Console.WriteLine();
                Console.WriteLine($"Deleted item with id: {idValue}");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Failed to delete item with id: {idValue}. Error: {ex.StatusCode} - {ex.Message}");
            }
        }
    }

    public static async Task UpsertRecordsAsync<T>(List<T> itemsToProcess, Container container)
    {
        foreach (var item in itemsToProcess)
        {
            var propertyInfo = typeof(T).GetProperty("Id");
            string? idValue = propertyInfo?.GetValue(item)?.ToString();

            try
            {
                await container.UpsertItemAsync(item, new PartitionKey(idValue));
                Console.WriteLine();          

                Console.WriteLine($"Updated item with id: {idValue}");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Failed to update item with id: {idValue}. Error: {ex.StatusCode} - {ex.Message}");
            }
        }
    }
}
