using Spreedly.Net.Model;
using Spreedly.Net.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Spreedly.Net.Resources;

namespace Test.Spreedly.Net
{
    [TestClass]
    public class ParsePingTests
    {
        private SpreedlyService _client;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var secrets = Secrets.keys_secret.Split('\t');
            _client = new SpreedlyService(secrets[0], secrets[1], secrets.Length > 2 ? secrets[2] : "", secrets.Length > 3 ? secrets[3] : "");
        }

        [TestMethod]
        public void when_we_ping_parse()
        {
            var result = _client.Ping();
            Assert.AreEqual(AsyncCallFailureReason.None, result);
        }
        


    }

}
