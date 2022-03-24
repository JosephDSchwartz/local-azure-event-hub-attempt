//using Amqp;

//Address address = new Address("amqp://localhost:5672");
//Connection connection = new Connection(address); // await Connection.Factory.CreateAsync(address);
//Session session = new Session(connection);

//Message message = new Message("Hello AMQP");
//SenderLink sender = new SenderLink(session, "sender-link", "q1");
//await sender.SendAsync(message);

//ReceiverLink receiver = new ReceiverLink(session, "receiver-link", "q1");
//message = await receiver.ReceiveAsync();
//receiver.Accept(message);

//await sender.CloseAsync();
//await receiver.CloseAsync();
//await session.CloseAsync();
//await connection.CloseAsync();



//using LocalConsoleEventHub;

//var broker = new TestAmqpBroker(new[] { "amqps://localhost:5671" }, null, "localhost", null);
//broker.Start();

//Console.WriteLine("Broker started. Press the enter key to exit...");
//Console.ReadLine();

//broker.Stop();
//Console.WriteLine("Broker stopped");


using Amqp;
using Amqp.Listener;
using Amqp.Types;
using LocalConsoleEventHub;

Address address = new Address("amqps://localhost:5671");
if (args.Length > 0)
{
    address = new Address(args[0]);
}

// uncomment the following to write frame traces
Trace.TraceLevel = TraceLevel.Frame;
Trace.TraceListener = (l, f, a) => Console.WriteLine(DateTime.Now.ToString("[hh:mm:ss.fff]") + " " + string.Format(f, a));

var cert = TestAmqpBroker.GetCertificate("localhost");

ContainerHost host = new ContainerHost(new[] { address }, cert);

host.Listeners.ToList().ForEach(listener =>
{
    listener.AMQP.MaxFrameSize = 1048576;

    listener.SASL.EnableAnonymousMechanism = true;
    listener.SASL.EnableMechanism(new Symbol(MsSbCbsSaslProfile.Name), new MsSbCbsSaslProfile());
});

host.Open();
Console.WriteLine("Container host is listening on {0}:{1}", address.Host, address.Port);

//host.RegisterLinkProcessor(new LinkProcessor());

string cbsAddress = "$cbs";
host.RegisterRequestProcessor(cbsAddress, new CbsRequestProcessor());
Console.WriteLine($"Request processor is registered on {cbsAddress}");

var hubAddress = "/TestHub";
host.RegisterRequestProcessor(hubAddress, new EventRequestProcessor());
Console.WriteLine($"Request processor is registered on {hubAddress}");

//host.RegisterMessageProcessor(hubAddress, new MessageProcessor());

Console.WriteLine("Press enter key to exit...");
Console.ReadLine();

host.Close();


class MessageProcessor : IMessageProcessor
{
    public void Process(MessageContext messageContext)
    {
        var thing = 0;
    }

    public int Credit => 1000;
}

class LinkProcessor : ILinkProcessor
{
    public void Process(AttachContext attachContext)
    {
        attachContext.Attach.MaxMessageSize = long.MaxValue;
    }
}


//using Amqp;
//using Amqp.Framing;
//using LocalConsoleEventHub;

//var url = "amqps://localhost:5671";
//String requestQueueName = "service_queue";

//Connection connection = null;

//try
//{
//    Address address = new Address(url);
//    connection = new Connection(address, new MsSbCbsSaslProfile(), null, null);
//    Session session = new Session(connection);

//    // Create server receiver link
//    // When messages arrive, reply to them
//    ReceiverLink receiver = new ReceiverLink(session, "Test-Event_Hub", requestQueueName);
//    int linkId = 0;
//    while (true)
//    {
//        Message request = receiver.Receive();
//        if (null != request)
//        {
//            receiver.Accept(request);
//            String replyTo = request.Properties.ReplyTo;
//            SenderLink sender = new SenderLink(session, "Interop.Server-sender-" + (++linkId).ToString(), replyTo);

//            Message response = new Message
//            {
//                ApplicationProperties = new ApplicationProperties
//                {
//                    Map = { { "status-code", 200 } }
//                }
//            };
//            response.Properties = new Properties() { CorrelationId = request.Properties.MessageId };

//            try
//            {
//                sender.Send(response);
//            }
//            catch (Exception exception)
//            {
//                Console.Error.WriteLine("Error waiting for response to be sent: {0}",
//                    exception.Message);
//                break;
//            }
//            sender.Close();
//        }
//        else
//        {
//            // timed out waiting for request. This is normal.
//            Console.WriteLine("Timeout waiting for request. Keep waiting...");
//        }
//    }
//}
//catch (Exception e)
//{
//    Console.Error.WriteLine("Exception {0}.", e);
//    if (null != connection)
//    {
//        connection.Close();
//    }
//}
//return 1;