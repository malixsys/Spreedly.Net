using Spreedly.Net.BuiltIns;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Spreedly.Net.Model
{
    internal interface IAsyncClient
    {
        // gateways
        Task<HttpResponseMessage> Gateways(CancellationToken token);
        Task<HttpResponseMessage> AddGateway(CancellationToken token, string type, Dictionary<string, string> otherGatewayInfos);
        Task<HttpResponseMessage> UpdateGateway(CancellationToken token, string gatewayToken, Dictionary<string, string> otherGatewayInfos);
        Task<HttpResponseMessage> Redact(CancellationToken token, string gatewayToken);

        // payments
        Task<HttpResponseMessage> ProcessPayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency);
        Task<HttpResponseMessage> AuthorizePayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency, bool retainOnSuccess = false);

        // transactions
        Task<HttpResponseMessage> TransactionsByGateway(CancellationToken token, string gatewayToken, string sinceToken = "");
        Task<HttpResponseMessage> Void(CancellationToken token, string transactionToken);
        Task<HttpResponseMessage> Transaction(CancellationToken token, string transactionToken);

        // payment methods
        Task<HttpResponseMessage> PaymentMethods(CancellationToken token);
        Task<HttpResponseMessage> ShowPaymentMethod(CancellationToken token, string paymentMethodToken);

        /// <summary>
        /// this method should only be used for testing due to PCI compliance.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="creditCard"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> AddPaymentMethod(CancellationToken token, CreditCard creditCard, string environmentKey);


        // payment method storage
        Task<HttpResponseMessage> RetainPaymentMethod(CancellationToken token, string paymentMethodToken);
        Task<HttpResponseMessage> RedactPaymentMethod(CancellationToken token, string paymentMethodToken);
        Task<HttpResponseMessage> VerifyPaymentMethod(CancellationToken token, string gatewayToken, string paymentMethodToken, bool retainOnSuccess = false);
        
    }
}