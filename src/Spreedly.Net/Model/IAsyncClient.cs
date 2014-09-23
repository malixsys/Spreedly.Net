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

        Task<HttpResponseMessage> TransactionsByGateway(CancellationToken token, string gatewayToken, string sinceToken = "");

        Task<HttpResponseMessage> Redact(CancellationToken token, string gatewayToken);
        Task<HttpResponseMessage> ProcessPayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency);
        Task<HttpResponseMessage> AuthorizePayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency, bool retainOnSuccess = false);
        Task<HttpResponseMessage> RetainPaymentMethod(CancellationToken token, string paymentMethodToken);
        Task<HttpResponseMessage> VerifyPaymentMethod(CancellationToken token, string gatewayToken, string paymentMethodToken, bool retainOnSuccess = false);
        Task<HttpResponseMessage> Void(CancellationToken token, string transactionToken);

        Task<HttpResponseMessage> Transaction(CancellationToken token, string transactionToken);
    }
}