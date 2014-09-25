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
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private readonly SecurityKeys _securityKeys;
        
        public SpreedlyService(string applicationId, string masterKey, string gatewayToken, string redactedToken)
            : this(new SecurityKeys(applicationId, masterKey, gatewayToken, redactedToken))
        {
        }

        private SpreedlyService(SecurityKeys securityKeys)
        {
            _securityKeys = securityKeys;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        #region Gateways

        /// <summary>
        /// List all gateways.
        /// </summary>
        public IEnumerable<Gateway> Gateways()
        {
            var result = Call((client, token) => client.Gateways(token));
            if(result.Failed())
            {
                return null;
            }

            return Gateway.FromXml(result.Contents);
        }

        /// <summary>
        /// Add a gateway. If it already exists return the existing gateway.
        /// </summary>
        public Gateway AddGateway(string type, Dictionary<string, string> otherGatewayInfos = null)
        {
            var gateway = GetEnabledGateway(type);
            if (gateway == null)
            {
                var result = Call((client, token) => client.AddGateway(token, type, otherGatewayInfos));
                if (result.Failed())
                {
                    log.Error(result.Contents.ToString());
                    return null;
                }
                gateway = Gateway.FromXml(result.Contents).FirstOrDefault();
                if(gateway != null)
                {
                    _securityKeys.LastGatewayToken = gateway.Token;
                }
            }
            return gateway;
        }

        public Gateway UpdateGateway(string gatewayToken, Dictionary<string, string> otherGatewayInfos = null)
        {
            var result = Call((client, token) => client.UpdateGateway(token, gatewayToken, otherGatewayInfos));
            if (result.Failed())
            {
                log.Error(result.Contents.ToString());
                return null;
            }
            var gateway = Gateway.FromXml(result.Contents).FirstOrDefault();
            if (gateway != null)
            {
                _securityKeys.LastGatewayToken = gateway.Token;
            }
            return gateway;
        }

        public Gateway GetEnabledGateway(string type)
        {
            var gateways = Gateways();
            return gateways.FirstOrDefault(g => g.Type == type && g.Enabled);
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
                log.Error(result.Contents.ToString());
                return null;
            }
            return Gateway.FromXml(result.Contents).FirstOrDefault();
        }

        #endregion

        #region Payments

        public Transaction ProcessPayment(string type, string paymentMethodToken, decimal amount, string currency)
        {
            var wasTest = string.Equals(type, "test", StringComparison.InvariantCultureIgnoreCase);
            var gateway = GetEnabledGateway(type);
             if (gateway == null)
             {
                 return new Transaction(wasTest,
                     new TransactionErrors("", TransactionErrorType.InvalidGateway));
             }
            var result = Call((client, token) => client.ProcessPayment(token, gateway.Token, paymentMethodToken, amount, currency));

            return HandleResult(result, wasTest);
        }

        public Transaction AuthorizePayment(string type, string paymentMethodToken, decimal amount, string currency, bool retainOnSuccess = false)
        {
            var wasTest = string.Equals(type, "test", StringComparison.InvariantCultureIgnoreCase);
            var gateway = GetEnabledGateway(type);
            if (gateway == null)
            {
                return new Transaction(wasTest,
                    new TransactionErrors("", TransactionErrorType.InvalidGateway));
            }
            var result = Call((client, token) => client.AuthorizePayment(token, gateway.Token, paymentMethodToken, amount, currency, retainOnSuccess));

            return HandleResult(result, wasTest);
        }

        #endregion

        #region Transactions

        public IEnumerable<Transaction> TransactionsByGateway(string gatewayToken, string sinceToken = "")
        {
            var result = Call((client, token) => client.TransactionsByGateway(token, gatewayToken, sinceToken));
            if (result.Failed())
            {
                return null;
            }

            return Transaction.ListFromXml(result.Contents);
        }

        public Transaction ShowTransaction(string transactionToken)
        {
            var result = Call((client, token) => client.Transaction(token, transactionToken));
            if (result.Failed())
            {
                return null;
            }

            return Transaction.FromXml(result.Contents);
        }

        public Transaction Void(string type, string transactionToken)
        {
            var wasTest = string.Equals(type, "test", StringComparison.InvariantCultureIgnoreCase);
            var gateway = GetEnabledGateway(type);
            if (gateway == null)
            {
                return new Transaction(wasTest,
                    new TransactionErrors("", TransactionErrorType.InvalidGateway));
            }
            var result = Call((client, token) => client.Void(token, transactionToken));

            return HandleResult(result, wasTest);
        }

        #endregion

        #region Payment Methods

        public IEnumerable<PaymentMethod> PaymentMethods()
        {
            var result = Call((client, token) => client.PaymentMethods(token));
            if (result.Failed())
            {
                return null;
            }

            return PaymentMethod.ListFromXml(result.Contents);
        }

        public PaymentMethod ShowPaymentMethod(string paymentMethodToken)
        {
            var result = Call((client, token) => client.ShowPaymentMethod(token, paymentMethodToken));
            return PaymentMethod.FromXml(result.Contents);
        }

        public Transaction AddPaymentMethod(CreditCard creditCard)
        {
            var result = Call((client, token) => client.AddPaymentMethod(token, creditCard, this._securityKeys.Credentials.UserName));

            return HandleResult(result, false);
        }

        public Transaction RetainPaymentMethod(string paymentMethodToken)
        {
            var result = Call((client, token) => client.RetainPaymentMethod(token, paymentMethodToken));

            return HandleResult(result, false);
        }

        public Transaction RedactPaymentMethod(string paymentMethodToken)
        {
            var result = Call((client, token) => client.RedactPaymentMethod(token, paymentMethodToken));

            return HandleResult(result, false);
        }

        public Transaction VerifyPaymentMethod(string type, string paymentMethodToken, bool retainOnSuccess = false)
        {
            var wasTest = string.Equals(type, "test", StringComparison.InvariantCultureIgnoreCase);
            var gateway = GetEnabledGateway(type);
            if (gateway == null)
            {
                return new Transaction(wasTest,
                    new TransactionErrors("", TransactionErrorType.InvalidGateway));
            }
            var result = Call((client, token) => client.VerifyPaymentMethod(token, gateway.Token, paymentMethodToken, retainOnSuccess));

            return HandleResult(result, wasTest);
        }

        #endregion

        #region Helper Methods

        private Transaction HandleResult(AsyncCallResult<XDocument> result, bool wasTest)
        {
            if (result.Failed())
            {
                log.Error("Spreedly transaction failed.");
                if (null != result && null != result.Contents)
                    log.Error(result.Contents.ToString());

                return null;
            }
            else
            {
                if (null != result && null != result.Contents)
                    log.Debug(result.Contents.ToString());
            }

            if (result.Contents == null)
            {
                return new Transaction(wasTest, new TransactionErrors("", TransactionErrorType.CallFailed));
            }

            return Transaction.FromXml(result.Contents);
        }

        internal bool CallNoReturn(string className, object criteria, Func<IAsyncClient, CancellationToken, Task<HttpResponseMessage>> innerCall)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            using (var client = new AsyncClient())
            {
                client.Init(_securityKeys.Credentials);
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
            if (result.FailureReason != AsyncCallFailureReason.None)
            {
                return result.FailureReason;
            }
            return result.Contents.Descendants("gateways").Any() ? AsyncCallFailureReason.None : AsyncCallFailureReason.ResultsNotFound;
        }

        private AsyncCallResult<XDocument> Call(Func<IAsyncClient, CancellationToken, Task<HttpResponseMessage>> innerCall_)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            using (var client = new AsyncClient())
            {
                client.Init(_securityKeys.Credentials);
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
                        var doc = XDocument.Load(streamReader, LoadOptions.SetLineInfo);
                        if (task.Result.IsSuccessStatusCode == false)
                        {
                            return new AsyncCallResult<XDocument>(AsyncCallFailureReason.FailedStatusCode, doc);
                        }
                        return new AsyncCallResult<XDocument>(AsyncCallFailureReason.None, doc);
                    }
                }
            }
        }

        #endregion

    }
}