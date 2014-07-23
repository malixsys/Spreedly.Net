using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Spreedly.Net.Model
{
    internal interface IAsyncClient
    {
        Task<HttpResponseMessage> Gateways(CancellationToken token);
        Task<HttpResponseMessage> Gateways(CancellationToken token, string type, Dictionary<string, string> otherGatewayInfos);
        Task<HttpResponseMessage> Redact(CancellationToken token, string gatewayToken);
        Task<HttpResponseMessage> ProcessPayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency);
        Task<HttpResponseMessage> RetainPaymentMethod(CancellationToken token, string paymentMethodToken);
    }
}