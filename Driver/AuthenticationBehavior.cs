using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Driver
{
    public class AuthenticationBehavior : IEndpointBehavior
    {
        readonly WorkdayCredentialsOptions _opts;

        public AuthenticationBehavior(WorkdayCredentialsOptions opts)
        {
            _opts = opts ?? throw new ArgumentNullException(nameof(opts));
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new SecurityInspector(_opts));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            return;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            return;
        }
    }

    public class SecurityInspector : IClientMessageInspector
    {
        readonly WorkdayCredentialsOptions _opts;

        public SecurityInspector(WorkdayCredentialsOptions opts)
        {
            _opts = opts ?? throw new ArgumentNullException(nameof(opts));
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var security = new SecurityHeader
            {
                UsernameToken = new UsernameToken
                {
                    Username = _opts.Username,
                    Password = _opts.Password
                }
            };

            request.Headers.Add(security);

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var security = reply.Headers.FirstOrDefault(h => h.Name.Equals(SecurityHeader.HeaderName));
            if (security != null)
            {
                reply.Headers.UnderstoodHeaders.Add(security);
            }
        }
    }

    [XmlRoot(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
    public class UsernameToken
    {
        [XmlAttribute(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
        public string Id { get; set; }

        [XmlElement]
        public string Username { get; set; }
        [XmlElement]
        public string Password { get; set; }
    }

    public class SecurityHeader : MessageHeader
    {
        public const string HeaderName = "Security";

        public UsernameToken UsernameToken { get; set; }

        public override string Name => HeaderName;

        public override string Namespace => "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

        public override bool MustUnderstand => true;

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UsernameToken));
            serializer.Serialize(writer, this.UsernameToken);
        }
    }
}
