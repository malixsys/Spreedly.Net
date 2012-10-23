using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Spreedly.Net.Model
{
    internal class AsyncClient : IDisposable, IAsyncClient
    {
        private const string PARSE_ROOT_URL = "https://spreedlycore.com/v1";
        private HttpClientHandler _handler;
        private HttpClient _client;
        private bool _disposed;


        internal AsyncClient(ICredentials credentials)
        {
            _handler = new HttpClientHandler { Credentials = credentials };
            _client = new HttpClient(_handler);
        }

      


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_client != null)
                {
                    var hc = _client;
                    _client = null;
                    hc.Dispose();
                }
                if (_handler != null)
                {
                    var hh = _handler;
                    _handler = null;
                    hh.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion IDisposable Members

        public Task<HttpResponseMessage> Gateways(CancellationToken token)
        {
            return _client.GetAsync(new Uri(PARSE_ROOT_URL + "/gateways.xml"), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> Gateways(CancellationToken token_, string type)
        {
            var xml = string.Format("<gateway><gateway_type>{0}</gateway_type></gateway>", type);
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(PARSE_ROOT_URL + "/gateways.xml"));
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return _client.SendAsync(request,  HttpCompletionOption.ResponseContentRead, token_);
        }

        public Task<HttpResponseMessage> Redact(CancellationToken token_, string gatewayToken_)
        {
            var uri = string.Format(PARSE_ROOT_URL + "/gateways/{0}/redact.xml", gatewayToken_);
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri));
            var content = new StringContent("");
            request.Content = content;
            return _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token_);
        }

        public Task<HttpResponseMessage> ProcessPayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency)
        {
            var uri = string.Format(PARSE_ROOT_URL + "/gateways/{0}/purchase.xml", gatewayToken);
            var xml = string.Format("<transaction><amount>{0:#.00}</amount><currency_code>{1}</currency_code><payment_method_token>{2}</payment_method_token></transaction>", 
                amount, currency, paymentMethodToken);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token);
        }
    }
}