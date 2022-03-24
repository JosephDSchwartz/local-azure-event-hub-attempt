using static System.Console;
using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;


// Let the Event Hub Project finish starting up.
Thread.Sleep(1000);

// Create a producer client that you can use to send events to an event hub
producerClient = new EventHubProducerClient(connectionString, eventHubName);

// Create a batch of events 
// MaximumSizeInBytes must be greater than or equal to 24 and less than long.MaxValue
// Or at least that's what the max size should be. Instead there is code in the Microsoft.Azure.Amqp library defaulting the max to ulong.MaxValue
// https://github.com/Azure/azure-amqp/blob/master/src/AmqpExtensions.cs#L122
// That causes problems later when it's converted to a long and then checked against here https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/src/Amqp/AmqpProducer.cs#L312.
//
// Need to figure out how to set that MaximumMessageSize value. It seems like it can maybe be set at attach time.
using EventDataBatch eventBatch = await producerClient.CreateBatchAsync(new CreateBatchOptions{MaximumSizeInBytes = 30});

for (int i = 1; i <= numOfEvents; i++)
{
    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event {i}"))))
    {
        // if it is too large for the batch
        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
    }
}

try
{
    // Use the producer client to send the batch of events to the event hub
    await producerClient.SendAsync(eventBatch);
    WriteLine($"A batch of {numOfEvents} events has been published.");
}
catch (Exception ex)
{
    WriteLine($"An error occurred: {ex.Message}");
    WriteLine(ex);
}
finally
{
    await producerClient.DisposeAsync();
}

public partial class Program
{
    // connection string to the Event Hubs namespace
    //const string connectionString = "Endpoint=sb://<NamespaceName>.servicebus.windows.net/;SharedAccessKeyName=<KeyName>;SharedAccessKey=<KeyValue>;EntityPath=<EventHubName>";
    // Endpoint=sb://pegatest1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wY4jn4l5h+m55Wu8yk2wSESGdAU5GcKUgyprshgbogc=
    private const string connectionString = "Endpoint=sb://localhost:5671/;SharedAccessKeyName=Primary;SharedAccessKey=123abc";

    // name of the event hub
    private const string eventHubName = "TestHub";

    // number of events to be sent to the event hub
    private const int numOfEvents = 3;

    // The Event Hubs client types are safe to cache and use as a singleton for the lifetime
    // of the application, which is best practice when events are being published or read regularly.
    private static EventHubProducerClient producerClient;
}
