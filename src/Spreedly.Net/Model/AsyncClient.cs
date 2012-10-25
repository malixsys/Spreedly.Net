﻿using System;
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
        private const string ROOT_URL = "https://spreedlycore.com/v1";


        public Task<HttpResponseMessage> Gateways(CancellationToken token)
        {
            return Client.GetAsync(new Uri(ROOT_URL + "/gateways.xml"), HttpCompletionOption.ResponseContentRead, token);
        }

        public Task<HttpResponseMessage> Gateways(CancellationToken token_, string type, Dictionary<string, string> otherGatewayInfos = null)
        {
            var xml = string.Format("<gateway><gateway_type>{0}</gateway_type>{1}</gateway>", type, DicToXml(otherGatewayInfos));
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(ROOT_URL + "/gateways.xml"));
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(request,  HttpCompletionOption.ResponseContentRead, token_);
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

        public Task<HttpResponseMessage> Redact(CancellationToken token_, string gatewayToken_)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}/redact.xml", gatewayToken_);
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(uri));
            var content = new StringContent("");
            request.Content = content;
            return Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token_);
        }

        public Task<HttpResponseMessage> ProcessPayment(CancellationToken token, string gatewayToken, string paymentMethodToken, decimal amount, string currency)
        {
            var uri = string.Format(ROOT_URL + "/gateways/{0}/purchase.xml", gatewayToken);
            var xml = string.Format("<transaction><amount>{0:#.00}</amount><currency_code>{1}</currency_code><payment_method_token>{2}</payment_method_token></transaction>", 
                amount, currency, paymentMethodToken);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new StringContent(xml, null, "application/xml");
            request.Content = content;
            return Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token);
        }

        public void Init(NetworkCredential credentials)
        {
            Init(ROOT_URL, credentials);
        }
    }
}