using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Spreedly.Net.BuiltIns;
using Spreedly.Net.Extensions;
using Spreedly.Net.Model;

namespace Spreedly.Net.Service
{
    public class SpreedlyService
    {
        private readonly SecurityKeys _securityKeys;
        
        public SpreedlyService(string applicationId, string masterKey, string gatewayToken, string redactedToken)
            : this(new SecurityKeys(applicationId, masterKey, gatewayToken, redactedToken))
        {
        }

        private SpreedlyService(SecurityKeys securityKeys_)
        {
            _securityKeys = securityKeys_;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }


        internal bool CallNoReturn(string className, object criteria, Func<IAsyncClient, CancellationToken, Task<HttpResponseMessage>> innerCall)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            using (var client = new AsyncClient(_securityKeys.Credentials))
            {
                using (var task = innerCall(client, token))
                {
                    if (task.Wait(30000, token) == false)
                    {
                        if (token.CanBeCanceled)
                        {
                            source.Cancel();
                        }
                        return false;
                    }
                    return task.Result.IsSuccessStatusCode;
                }
            }
        }


        public AsyncCallFailureReason Ping()
        {
            var result = Call((client, token) => client.Gateways(token));
            if(result.FailureReason != AsyncCallFailureReason.None)
            {
                return result.FailureReason;
            }
            return result.Contents.Descendants("gateways").Any() ? AsyncCallFailureReason.None : AsyncCallFailureReason.ResultsNotFound;
        }

        private AsyncCallResult<XDocument> Call(Func<IAsyncClient, CancellationToken, Task<HttpResponseMessage>> innerCall_)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            using (var client = new AsyncClient(_securityKeys.Credentials))
            {
                using (var task = innerCall_(client, token))
                {
                    try
                    {
                        if (task.Wait(5000, token) == false)
                        {
                            if (token.CanBeCanceled)
                            {
                                source.Cancel();
                            }
                            return new AsyncCallResult<XDocument>(AsyncCallFailureReason.TimeOut);
                        }
                    }
                    catch (Exception)
                    {
                        return new AsyncCallResult<XDocument>(AsyncCallFailureReason.FailedConnection);
                    }
                    var content = task.Result.Content.ReadAsStreamAsync();
                    if (content.Wait(250, token) == false)
                    {
                        if (token.CanBeCanceled)
                        {
                            source.Cancel();
                        }
                        return new AsyncCallResult<XDocument>(AsyncCallFailureReason.TimeOut);
                    }

                    using (var streamReader = new StreamReader(content.Result))
                    {
                        var doc = XDocument.Load(streamReader,LoadOptions.SetLineInfo);
                        if (task.Result.IsSuccessStatusCode == false)
                        {
                            return new AsyncCallResult<XDocument>(AsyncCallFailureReason.FailedStatusCode, doc);
                        }
                        return new AsyncCallResult<XDocument>(AsyncCallFailureReason.None, doc);
                    }
                }
            }
        }

        public IEnumerable<Gateway> Gateways()
        {
            var result = Call((client, token) => client.Gateways(token));
            if(result.Failed())
            {
                return null;
            }

            return Gateway.FromXml(result.Contents);
        }

        

        public Gateway AddGateway(string type)
        {
            var gateway = GetGateway(type, _securityKeys.GatewayToken);
            if (gateway == null)
            {
                var result = Call((client, token) => client.Gateways(token, type));
                if (result.Failed())
                {
                    return null;
                }
                gateway = Gateway.FromXml(result.Contents).FirstOrDefault();
                if(gateway != null)
                {
                    _securityKeys.GatewayToken = gateway.Token;
                }
            }
            return gateway;
        }

        public Gateway GetGateway(string type_, string token_)
        {
            var gateways = Gateways();
            return gateways.FirstOrDefault(g => g.Type == type_ && g.Token == token_ && g.Enabled);
        }

        public string RedactedToken()
        {
            return _securityKeys.RedactedTokens.FirstOrDefault();
        }

        public Gateway RedactGateway(string gatewayToken)
        {
            var result = Call((client, token) => client.Redact(token, gatewayToken));
            if (result.Failed())
            {
                return null;
            }
            return Gateway.FromXml(result.Contents).FirstOrDefault();
        }

        public Transaction ProcessPayment(string type, string paymentMethodToken_, decimal amount, string currency)
        {
            var wasTest = string.Equals(type, "test", StringComparison.InvariantCultureIgnoreCase);
            var gateway = GetGateway(type, _securityKeys.GatewayToken);
             if (gateway == null)
             {
                 return new Transaction(wasTest,
                     new TransactionErrors("", TransactionErrorType.InvalidGateway));
             }
            var result = Call((client, token) => client.ProcessPayment(token, gateway.Token, paymentMethodToken_, amount, currency));
            if(result.Contents == null)
            {
                return new Transaction(wasTest,new TransactionErrors("", TransactionErrorType.CallFailed));
            }
            return Transaction.FromXml(result.Contents);
        }
    }
}