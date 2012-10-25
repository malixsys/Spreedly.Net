using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Xml.Linq;
using Spreedly.Net.BuiltIns;
using Spreedly.Net.Model;
using Spreedly.Net.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Spreedly.Net.Resources;

namespace Test.Spreedly.Net
{
    [TestClass]
    public class ParsePaymentTests
    {
        private SpreedlyService _client;
        private string _login;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var secrets = Secrets.spreedly.Split('\t');
            _login = secrets[0];
            _client = new SpreedlyService(secrets[0], secrets[1], secrets.Length > 2 ? secrets[2] : "", secrets.Length > 3 ? secrets[3] : "");
        }

        [TestMethod]
        public void when_we_submit_a_payment_form()
        {
            var tran = ProcessPayment("4111111111111111", "12");
            Assert.IsNotNull(tran);
            Assert.IsTrue(tran.Succeeded);
            Assert.IsTrue(tran.ObfuscatedNumber.EndsWith("1111"));
            Assert.AreEqual(100.00M, tran.Amount);
            Assert.IsTrue(tran.WasTest);
        }

        [TestMethod]
        public void when_we_submit_a_payment_form_without_optional()
        {
            var tran = ProcessPayment("4111111111111111", "");
            Assert.IsNotNull(tran);
            Assert.IsFalse(tran.Succeeded);
            Assert.IsTrue(tran.ObfuscatedNumber.EndsWith("1111"));
            Assert.AreEqual(100.00M, tran.Amount);
        }

        private Transaction ProcessPayment(string number_, string optional_)
        {
            var result = SendForm(number_, optional_);
            Assert.IsNotNull(result);
            Assert.AreEqual(AsyncCallFailureReason.None, result.FailureReason);
            var parts = result.Contents.Split('=');
            Assert.AreEqual(2, parts.Length);
            Assert.AreEqual("token", parts[0]);
            var paymentMethodToken = parts[1];
            Assert.IsFalse(string.IsNullOrWhiteSpace(paymentMethodToken));
            var tran = _client.ProcessPayment("test", paymentMethodToken, 100.00M, "CAD");

            return tran;
        }

        [TestMethod]
        public void when_we_submit_a_bad_payment_form()
        {
            var tran = ProcessPayment("4012888888881881", "12");
            Assert.IsNotNull(tran);
            Assert.IsFalse(tran.Succeeded);
            Assert.IsTrue(tran.Errors.Count > 0);

        }

        private AsyncCallResult<string> SendForm(string number, string optional)
        {
            var source = new CancellationTokenSource();
            var token = source.Token;

            using(var content = new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("redirect_url", "https://azoo.me/account/purchase_redirect_callback"),
                        new KeyValuePair<string, string>("api_login", _login),
                        new KeyValuePair<string, string>("credit_card[number]", number),
                        new KeyValuePair<string, string>("credit_card[month]", optional),
                        new KeyValuePair<string, string>("credit_card[year]", "2020"),
                        new KeyValuePair<string, string>("credit_card[verification_value]", "666"),
                        new KeyValuePair<string, string>("credit_card[first_name]", "Martin"),
                        new KeyValuePair<string, string>("credit_card[last_name]", "Alix")
                    }))
            using(var handler = new HttpClientHandler { AllowAutoRedirect = false, PreAuthenticate = false})
            using(var client = new HttpClient(handler))
            {
                using (var task = client.PostAsync("https://spreedlycore.com/v1/payment_methods", content, token))
                {
                    try
                    {
                        if (task.Wait(5000, token) == false)
                        {
                            if (token.CanBeCanceled)
                            {
                                source.Cancel();
                            }
                            return new AsyncCallResult<string>(AsyncCallFailureReason.TimeOut);
                        }
                    }
                    catch (Exception)
                    {
                        return new AsyncCallResult<string>(AsyncCallFailureReason.FailedConnection);
                    }
                    if (task.Result.StatusCode != HttpStatusCode.Redirect)
                    {
                        return new AsyncCallResult<string>(AsyncCallFailureReason.FailedStatusCode, task.Result.Content.ReadAsStringAsync().Result);
                    }

                    return new AsyncCallResult<string>(AsyncCallFailureReason.None, task.Result.Headers.Location.Query.Substring(1));
                }
            }
        }
        [TestMethod]
        public void when_we_generate_a_good_transaction()
        {
            var doc = XDocument.Parse(GOOD_TRANSACTION_XML);
            var t = Transaction.FromXml(doc);
            Assert.IsNotNull(t);
            Assert.AreEqual(100M, t.Amount);
            Assert.IsTrue(t.WasTest);
            Assert.IsTrue(t.Succeeded);
            Assert.AreEqual("7212OjIpGu839iDJa7iOoqxgQwb", t.Token);
            Assert.AreEqual("XXXX-XXXX-XXXX-1111", t.ObfuscatedNumber);
        }

        [TestMethod]
        public void when_we_generate_a_bad_transaction()
        {
            var doc = XDocument.Parse(BAD_TRANSACTION_XML);
            var t = Transaction.FromXml(doc);
            Assert.IsNotNull(t);
            Assert.AreEqual(100M, t.Amount);
            Assert.IsFalse(t.WasTest);
            Assert.IsFalse(t.Succeeded);
            Assert.AreEqual("N9kkzYiyHKCudFUpUAvkes8sxJA", t.Token);
            Assert.AreEqual("", t.ObfuscatedNumber);
            Assert.IsNotNull(t.Errors);
            Assert.AreEqual(6, t.Errors.Count);
            Assert.AreEqual(TransactionErrorType.Blank, t.Errors["first_name"][0]);
            Assert.AreEqual(TransactionErrorType.Blank, t.Errors["last_name"][0]);
            Assert.AreEqual(TransactionErrorType.Invalid, t.Errors["month"][0]);
            Assert.AreEqual(2, t.Errors["year"].Length);
            Assert.AreEqual(TransactionErrorType.Blank, t.Errors["number"][0]);
        }
        private string BAD_TRANSACTION_XML = @"<transaction>
  <amount type='integer'>100</amount>
  <on_test_gateway type='boolean'>false</on_test_gateway>
  <created_at type='datetime'>2012-10-18T22:22:22Z</created_at>
  <updated_at type='datetime'>2012-10-18T22:22:22Z</updated_at>
  <currency_code>USD</currency_code>
  <succeeded type='boolean'>false</succeeded>
  <state>failed</state>
  <token>N9kkzYiyHKCudFUpUAvkes8sxJA</token>
  <transaction_type>Purchase</transaction_type>
  <order_id nil='true'/>
  <ip nil='true'/>
  <description nil='true'/>
  <message key='messages.payment_method_invalid'>The payment method is invalid.</message>
  <gateway_token>LFZIIZpwptNNMfnSk4Zezr9QnMw</gateway_token>
  <payment_method>
    <token>9AyJ4agCzvaoL4gDXxuERcft803</token>
    <created_at type='datetime'>2012-10-18T22:22:22Z</created_at>
    <updated_at type='datetime'>2012-10-18T22:22:22Z</updated_at>
    <last_four_digits/>
    <card_type nil='true'/>
    <first_name/>
    <last_name/>
    <month nil='true'/>
    <year nil='true'/>
    <email nil='true'/>
    <address1 nil='true'/>
    <address2 nil='true'/>
    <city nil='true'/>
    <state nil='true'/>
    <zip nil='true'/>
    <country nil='true'/>
    <phone_number nil='true'/>
    <data nil='true'/>
    <payment_method_type>credit_card</payment_method_type>
    <verification_value/>
    <number/>
    <errors>
      <error attribute='first_name' key='errors.blank'>First name can't be blank</error>
      <error attribute='last_name' key='errors.blank'>Last name can't be blank</error>
      <error attribute='month' key='errors.invalid'>Month is invalid</error>
      <error attribute='year' key='errors.expired'>Year is expired</error>
      <error attribute='year' key='errors.invalid'>Year is invalid</error>
      <error attribute='number' key='errors.blank'>Number can't be blank</error>
    </errors>
  </payment_method>
</transaction>";
        private string GOOD_TRANSACTION_XML = @"<transaction>
  <amount type='integer'>100</amount>
  <on_test_gateway type='boolean'>true</on_test_gateway>
  <created_at type='datetime'>2012-10-18T22:22:22Z</created_at>
  <updated_at type='datetime'>2012-10-18T22:22:22Z</updated_at>
  <currency_code>USD</currency_code>
  <succeeded type='boolean'>true</succeeded>
  <state>succeeded</state>
  <token>7212OjIpGu839iDJa7iOoqxgQwb</token>
  <transaction_type>Purchase</transaction_type>
  <order_id nil='true'/>
  <ip nil='true'/>
  <description nil='true'/>
  <message key='messages.transaction_succeeded'>Succeeded!</message>
  <gateway_token>LFZIIZpwptNNMfnSk4Zezr9QnMw</gateway_token>
  <response>
    <success type='boolean'>true</success>
    <message>Successful purchase!</message>
    <avs_code nil='true'/>
    <avs_message nil='true'/>
    <cvv_code nil='true'/>
    <cvv_message nil='true'/>
    <error_code/>
    <error_detail nil='true'/>
    <created_at type='datetime'>2012-10-18T22:22:22Z</created_at>
    <updated_at type='datetime'>2012-10-18T22:22:22Z</updated_at>
  </response>
  <payment_method>
    <token>W8E74PidzpuwJUD4ANsHwqQsL69</token>
    <created_at type='datetime'>2012-10-18T22:22:22Z</created_at>
    <updated_at type='datetime'>2012-10-18T22:22:22Z</updated_at>
    <last_four_digits>1111</last_four_digits>
    <card_type>visa</card_type>
    <first_name>Bob</first_name>
    <last_name>Smith</last_name>
    <month type='integer'>1</month>
    <year type='integer'>2020</year>
    <email nil='true'/>
    <address1 nil='true'/>
    <address2 nil='true'/>
    <city nil='true'/>
    <state nil='true'/>
    <zip nil='true'/>
    <country nil='true'/>
    <phone_number nil='true'/>
    <data nil='true'/>
    <payment_method_type>credit_card</payment_method_type>
    <verification_value/>
    <number>XXXX-XXXX-XXXX-1111</number>
    <errors>
    </errors>
  </payment_method>
</transaction>";

        
    }

}
