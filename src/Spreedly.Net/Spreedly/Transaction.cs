﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Spreedly.Net.Extensions;

namespace Spreedly.Net.BuiltIns
{
    public class Transaction
    {
        internal Transaction(string raw, string amount, string wasTest, string succeeded, string token, string obfuscatedNumber, TransactionErrors errors)
        {
            if (!string.IsNullOrEmpty(amount))
                Amount = decimal.Parse(amount, CultureInfo.InvariantCulture);

            WasTest = string.Equals(wasTest, "true", StringComparison.InvariantCultureIgnoreCase);

            Succeeded = string.Equals(succeeded, "true", StringComparison.InvariantCultureIgnoreCase);
            Token = token;
            ObfuscatedNumber = obfuscatedNumber;
            Errors = errors;

            Raw = raw;
        }

        public Transaction(bool wasTest, TransactionErrors errors)
        {
            Succeeded = false;
            WasTest = wasTest;
            Errors = errors;
        }

        public static Transaction FromXml(XDocument doc)
        {
            var tran = doc.Element("transaction");
            return FromXml(tran);
        }

        public static Transaction FromXml(XElement tran)
        {
            if (tran == null)
            {
                return null;
            }
            var ret = new Transaction(
                tran.ToString(),
                tran.GetStringChild("amount"),
                tran.GetStringChild("on_test_gateway"),
                tran.GetStringChild("succeeded"),
                tran.GetStringChild("token"),
                tran.Element("payment_method").GetStringChild("number"),
                new TransactionErrors(tran)
            );

            ret.GatewayToken = tran.GetStringChild("gateway_token");
            ret.GatewayTransactionId = tran.GetStringChild("gateway_transaction_id");
            ret.CreatedAt = DateTime.Parse(tran.GetStringChild("created_at"));

            ret.Response = new TransactionResponse(tran.Element("response"));

            if (ret.Succeeded == false && ret.Errors.Count == 0)
            {
                if (string.Equals(tran.GetStringChild("state"), "gateway_processing_failed",
                                  StringComparison.InvariantCultureIgnoreCase))
                {

                    ret.Errors = new TransactionErrors("",TransactionErrorType.Unknown);
                }
            }
            return ret;
        }

        public static List<Transaction> ListFromXml(XDocument doc)
        {
            List<Transaction> transactions = new List<Transaction>();

            var trans = doc.Element("transactions").Elements("transaction");
            if (trans == null)
            {
                return transactions;
            }
            
            foreach (var tran in trans)
            {
                var ret = FromXml(tran);
                transactions.Add(ret);
            }

            return transactions;
        }


        public decimal Amount { get; private set; }

        public bool WasTest { get; private set; }

        public bool Succeeded { get; private set; }

        public string Token { get; private set; }

        public string ObfuscatedNumber { get; private set; }

        [Obsolete("Use Response.AvsCode instead")]
        public string AvsCode
        {
            get
            {
                if (Response != null)
                    return Response.AvsCode;
                return null;
            }
        }

        public TransactionErrors Errors { get; private set; }

        public string Raw { get; private set; }

        public string GatewayToken { get; set; }

        public string GatewayTransactionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public TransactionResponse Response { get; private set; }
    }

    public class TransactionErrors
    {
        private List<KeyValuePair<string, TransactionErrorType>> _list;

        public TransactionErrors(string key, TransactionErrorType type)
        {
            _list = new List<KeyValuePair<string, TransactionErrorType>>
                        {
                            new KeyValuePair<string, TransactionErrorType>(key, type)
                        };
        }
        public TransactionErrors(XElement tran)
        {
            var eles = tran.Descendants("error");
            _list =
                eles
                    .Select(ele => new KeyValuePair<string, TransactionErrorType>(GetKey(ele), GetErrorType(ele)))
                    .ToList();
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        private static string GetKey(XElement err)
        {
            var att = err.Attribute("attribute");
            return att == null ? string.Empty : att.Value;
        }

        private static TransactionErrorType GetErrorType(XElement err)
        {
            var att = err.Attribute("key");
            if (att == null) return TransactionErrorType.Unknown;
            switch (att.Value.ToLowerInvariant())
            {
                case "errors.blank":
                    return TransactionErrorType.Blank;
                case "errors.invalid":
                    return TransactionErrorType.Invalid;
                case "errors.expired":
                    return TransactionErrorType.Expired;
                default:
                    return TransactionErrorType.Unknown;
            }
        }

        public TransactionErrorType[] this[string key]
        {
            get
            {
                return
                    _list.Where(kv => string.Equals(kv.Key, key, StringComparison.InvariantCultureIgnoreCase)).Select(
                        kv => kv.Value).ToArray();
            }
        }
    }

    public enum TransactionErrorType
    {
        Unknown,
        Blank,
        Invalid,
        Expired,
        InvalidGateway,
        CallFailed
    }
}