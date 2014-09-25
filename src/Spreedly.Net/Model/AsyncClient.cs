using Spreedly.Net.BuiltIns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spreedly.Net.Model
{
    internal class AsyncClient : BaseAsyncClient, IAsyncClient
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        private const string ROOT_URL = "https://spreedlycore.com/v1";

        #region Gateways

        public Task<HttpResponseMessage> Gateways(CancellationToken token)
        {
            return Client.GetAsync(new Uri(ROOT_URL + "/gateways.xml"), HttpCompletionOption.ResponseContentRead, token);
        }
        
        public Task<HttpResponseMessage> AddGateway(CancellationToken token, string type, Dictionary<string, string> otherGatewayInfos = null)
        {
            var xml = string.Format("<gateway><gateway_type>{0}</gateway_type>{1}</gateway>", type, DicToXml(otherGatewayInfos));
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(ROOT_URL + "/gateways.xml"));
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request),  HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> UpdateGateway(CancellationToken token, string gatewayToken, Dictionary<string, string> otherGatewayInfos = null)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}.xml", gatewayToken);
            var xml = string.Format("<gateway>{0}</gateway>", DicToXml(otherGatewayInfos));
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri));
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> Redact(CancellationToken token, string gatewayToken)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}/redact.xml", gatewayToken);
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri));
            var content = new StringContent("");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        #endregion

        #region Transactions

        public Task<HttpResponseMessage> TransactionsByGateway(CancellationToken token, string gatewayToken, string sinceToken = "")
        {
            var url = string.Format(ROOT_URL + "/gateways/{0}/transactions.xml?order=desc", gatewayToken);

            if (!string.IsNullOrEmpty(sinceToken))
                url += string.Format("&since_token={0}", sinceToken);

            return Client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> Transaction(CancellationToken token, string transactionToken)
        {
            var url = string.Format(ROOT_URL + "/transactions/{0}.xml", transactionToken);
            return Client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> Void(CancellationToken token, string transactionToken)
        {
            var uri = string.Format(ROOT_URL + "/transactions/{0}/void.xml", transactionToken);

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent("", null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        #endregion

        #region Payment

        public Task<HttpResponseMessage> ProcessPayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}/purchase.xml", gatewayToken);

            // convert to amount in cents
            int amountInCents = Convert.ToInt32(amount * 100);

            var xml = string.Format("<transaction><amount>{0}</amount><currency_code>{1}</currency_code><payment_method_token>{2}</payment_method_token></transaction>", 
                amountInCents, currency, paymentMethodToken);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;


            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> AuthorizePayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency, bool retainOnSuccess = false)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}/authorize.xml", gatewayToken);

            // convert to amount in cents
            int amountInCents = Convert.ToInt32(amount * 100);

            var xml = string.Format("<transaction><amount>{0}</amount><currency_code>{1}</currency_code><payment_method_token>{2}</payment_method_token><retain_on_success>{3}</retain_on_success></transaction>",
                amountInCents, currency, paymentMethodToken, retainOnSuccess);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        #endregion

        #region Payment Methods

        public Task<HttpResponseMessage> AddPaymentMethod(CancellationToken token, CreditCard creditCard, string environmentKey)
        {
            var uri = string.Format(ROOT_URL + "/payment_methods.xml?environment_key={0}", environmentKey);
            var xml = string.Format("<payment_method>{0}</payment_method>", creditCard.ToXml());

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }


        public Task<HttpResponseMessage> ShowPaymentMethod(CancellationToken token, string paymentMethodToken)
        {
            var uri = string.Format(ROOT_URL + "/payment_methods/{0}.xml", paymentMethodToken);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }


        public Task<HttpResponseMessage> RetainPaymentMethod(CancellationToken token, string paymentMethodToken)
        {
            var uri = string.Format(ROOT_URL + "/payment_methods/{0}/retain.xml", paymentMethodToken);

            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            var content = new StringContent("", null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> RedactPaymentMethod(CancellationToken token, string paymentMethodToken)
        {
            var uri = string.Format(ROOT_URL + "/payment_methods/{0}/redact.xml", paymentMethodToken);

            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            var content = new StringContent("", null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> PaymentMethods(CancellationToken token)
        {
            return Client.GetAsync(new Uri(ROOT_URL + "/payment_methods.xml"), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> VerifyPaymentMethod(CancellationToken token, string gatewayToken, string paymentMethodToken, bool retainOnSuccess = false)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}/verify.xml", gatewayToken);

            var xml = string.Format(@"
                <transaction>
                    <payment_method_token>{0}</payment_method_token>
                    <retain_on_success>{1}</retain_on_success>
                </transaction>",
                paymentMethodToken, retainOnSuccess);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(LogRequest(request), HttpCompletionOption.ResponseContentRead, token);
        }

        #endregion

        #region Helper Methods

        public void Init(NetworkCredential credentials)
        {
            Init(ROOT_URL, credentials);
        }

        private string DicToXml(Dictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Any() == false)
                return string.Empty;
            var sb = new StringBuilder();
            foreach (var key in dictionary.Keys)
            {
                sb.AppendFormat("<{0}>{1}</{0}>", key, dictionary[key]);
            }
            return sb.ToString();
        }

        private HttpRequestMessage LogRequest(HttpRequestMessage request)
        {
            log.DebugFormat("{1} to URL: {0}", request.RequestUri, request.Method);
            if (null != request.Content)
            {
                log.DebugFormat(request.Content.ReadAsStringAsync().Result.ToString());
            }

            return request;
        }

        #endregion
    }
}