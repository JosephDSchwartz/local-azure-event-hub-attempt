using System.Text;
using Amqp;
using Amqp.Sasl;
using Amqp.Types;

namespace LocalConsoleEventHub
{
    internal class MsSbCbsSaslProfile : SaslProfile
    {
        public const string Name = "MSSBCBS";

        private const ulong SaslInitCode = 0x0000000000000041;
        private const ulong SaslMechanismsCode = 0x0000000000000040;

        public MsSbCbsSaslProfile() : base(Name)
        {
        }

        protected override ITransport UpgradeTransport(ITransport transport)
        {
            return transport;
        }

        protected override DescribedList GetStartCommand(string hostname)
        {
            return new SaslInit()
            {
                Mechanism = this.Mechanism,
                InitialResponse = Encoding.UTF8.GetBytes(Name)
            };
        }

        protected override DescribedList OnCommand(DescribedList command)
        {
            if (command.Descriptor.Code == SaslInitCode)
            {
                return new SaslOutcome() { Code = SaslCode.Ok };
            }
            else if (command.Descriptor.Code == SaslMechanismsCode)
            {
                return null;
            }

            throw new AmqpException(ErrorCode.NotAllowed, command.ToString());
        }
    }
}
