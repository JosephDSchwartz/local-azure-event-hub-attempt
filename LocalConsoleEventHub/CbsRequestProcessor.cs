using Amqp;
using Amqp.Framing;
using Amqp.Listener;

namespace LocalConsoleEventHub
{
    internal class CbsRequestProcessor : IRequestProcessor
    {
        int offset;

        int IRequestProcessor.Credit => 1000;

        void IRequestProcessor.Process(RequestContext requestContext)
        {
            Console.WriteLine("Received a request " + requestContext.Message.Body);
            var task = this.ReplyAsync(requestContext);
        }

        async Task ReplyAsync(RequestContext requestContext)
        {
            if (this.offset == 0)
            {
                var requestOffset = requestContext.Message.ApplicationProperties["offset"];
                this.offset = requestOffset == null ? 0 : (int)requestOffset;
            }

            while (this.offset < 1000)
            {
                try
                {
                    //Message response = new Message("reply" + this.offset);
                    //response.ApplicationProperties = new ApplicationProperties();
                    //response.ApplicationProperties["offset"] = this.offset;
                    var response = new Message
                    {
                        ApplicationProperties = new ApplicationProperties
                        {
                            Map = { { "status-code", 200 } }
                        },
                        Properties = new Properties
                        {
                            CorrelationId = requestContext.Message.Properties.MessageId
                        }
                    };

                    requestContext.ResponseLink.SendMessage(response);
                    this.offset++;
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception: " + exception.Message);
                    if (requestContext.State == ContextState.Aborted)
                    {
                        Console.WriteLine("Request is aborted. Last offset: " + this.offset);
                        return;
                    }
                }

                await Task.Delay(1000);
            }

            requestContext.Complete(new Message("done"));
        }
    }
}
