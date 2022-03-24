using Amqp.Listener;

namespace LocalConsoleEventHub
{
    internal class EventRequestProcessor : IRequestProcessor
    {
        public void Process(RequestContext requestContext)
        {
            Console.WriteLine($"\nReceived request:\n{requestContext.Message.Body}\n");
        }

        public int Credit => 1000;
    }
}
