# SvcUtil
There is a well known bug in dotnet-svcutil that generates bad properties/fields in ServiceReference generated code. https://github.com/dotnet/wcf/issues/1274

When you attempt to use that code, you are met with an arcane exception:

`Compiling JScript/CSharp scripts is not supported`

This exception comes from the dotnet core implementation of the XmlSerializer, which sees the incorrect type annotation and tries to fall back to a `RefEmit` implementation of serialization which is not implemented for dotnet core.

In a generated ServiceReference file, dotnet-svcutil will generate properties that look like:

```csharp
private Integration_Document_Option_ValueObjectType[][] // bug is here, XmlArrayItemAttribute specifies a single array, not a jagged array
...
/// <remarks/>
[System.Xml.Serialization.XmlArrayAttribute(Order=5)]
[System.Xml.Serialization.XmlArrayItemAttribute("Integration_Document_Option_Value_Reference", typeof(Integration_Document_Option_ValueObjectType), IsNullable=false)]
public Integration_Document_Option_ValueObjectType[][] Integration_Document_Field_Options // bug is here, XmlArrayItemAttribute specifies a single array, not a jagged array
{
    get
    {
        return this.integration_Document_Field_OptionsField;
    }
    set
    {
        this.integration_Document_Field_OptionsField = value;
    }
}
```

It should read like this instead:
```csharp
/// <remarks/>
private Integration_Document_Option_ValueObjectType[] // fixed
...
/// <remarks/>
[System.Xml.Serialization.XmlArrayAttribute(Order=5)]
[System.Xml.Serialization.XmlArrayItemAttribute("Integration_Document_Option_Value_Reference", typeof(Integration_Document_Option_ValueObjectType), IsNullable=false)]
public Integration_Document_Option_ValueObjectType[] Integration_Document_Field_Options // fixed
{
    get
    {
        return this.integration_Document_Field_OptionsField;
    }
    set
    {
        this.integration_Document_Field_OptionsField = value;
    }
}
```

## Automation
A prototypical dotnet-svcutil call looks like this:

```powershell
cd <project>
dotnet-svcutil.exe https://wd5-impl-services1.workday.com/ccx/service/uw11/Integrations/v35.0/?wsdl --serializer XmlSerializer
```

We wrap this in a powershell script `gen.ps1` that rewrites the file as follows:
- Scan file for lines that start with "public" and contain "[][]".
  - This indicates we've landed on a jagged array property.
- Rewind through the class until we've found the `XmlArrayItemAttribute` for this property and calculate how many bracket pairs there.
  - This count + 1 is how many the property and it's backing field should have.
- Rewind through the class until we've found the backing field for this property.
  - Correct its contents so that the type has the correct number of bracket pairs.
- Correct the property line so that the type has the correct number of bracket pairs.

## Success
Once you've corrected the file, we need to implement some interfaces to allow .NET Core to authenticate it's requests with the very legacy WSS:Security header that Workday relies on.

Here is a complete working snippet that gets past the authentication stage and fails request validation.

```csharp
using ServiceReference;
using System;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Text.Json;
using System.Xml.Serialization;
using System.Xml;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.Linq;

namespace WSDL
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new IntegrationsPortClient();
            client.Endpoint.EndpointBehaviors.Add(new AuthenticationBehavior(new WorkdayOptions{ Username = "<enter username>", Password = "<enter password>"}));

            try
            {
                var t = await client.Get_Import_ProcessesAsync(new Workday_Common_HeaderType(),
                    new Get_Import_Processes_RequestType
                    {
                        version = "v35.0"
                    });

                Console.WriteLine(JsonSerializer.Serialize(t));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    class AuthenticationBehavior : IEndpointBehavior
    {
        readonly WorkdayOptions _opts;

        public AuthenticationBehavior(WorkdayOptions opts)
        {
            _opts = opts;
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


    class SecurityInspector : IClientMessageInspector
    {
        readonly WorkdayOptions _opts;

        public SecurityInspector(WorkdayOptions opts)
        {
            _opts = opts;
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

            return null; // correlationState
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var security = reply.Headers.FirstOrDefault(h => h.Namespace.Equals("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"));
            if (security != null)
            {
                reply.Headers.UnderstoodHeaders.Add(security);
            }
        }
    }

    class WorkdayOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
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
        public UsernameToken UsernameToken { get; set; }

        public override string Name => "Security";

        public override string Namespace => "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

        public override bool MustUnderstand => true;

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UsernameToken));
            serializer.Serialize(writer, this.UsernameToken);
        }
    }
}
```