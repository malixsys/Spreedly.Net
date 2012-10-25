using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spreedly.Net.Model;
using Spreedly.Net.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Spreedly.Net.Resources;

namespace Test.Spreedly.Net
{
    [TestClass]
    public class StripeTests
    {
        private GenericService<StripeClient> _service;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var secrets = Secrets.stripe.Split('\t');
            var serializer = new JsonSerializer();
            _service = new GenericService<StripeClient>("https://api.stripe.com/v1/", secrets[0], secrets[1], serializer);
        }

        [TestMethod]
        public void when_we_retrieve_accounts()
        {
            var result = _service.Call<dynamic>((client,token) => client.SimpleGet(token, "account"));
            Assert.AreEqual(AsyncCallFailureReason.None, result.FailureReason);
        }
        


    }

    internal class StripeClient:BaseAsyncClient
    {
    }
}
