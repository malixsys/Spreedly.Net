using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Spreedly.Net.Model
{
    public class BaseAsyncClient : IDisposable
    {
        private bool _initialized;
        private string _rootUrl;
        private HttpClientHandler _handler;
        protected HttpClient Client;
        private bool _disposed;


        protected BaseAsyncClient()
        {
        }


        protected internal void Init(string rootUrl, ICredentials credentials)
        {
            if (_initialized) throw new InvalidOperationException("Already initialized");
            _initialized = true;
            _rootUrl = rootUrl;
            _handler = new HttpClientHandler { Credentials = credentials };
            Client = new HttpClient(_handler);
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
                if (Client != null)
                {
                    var hc = Client;
                    Client = null;
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

        public Task<HttpResponseMessage> SimpleGet(CancellationToken token, string path)
        {
            return Client.GetAsync(new Uri(_rootUrl + path), HttpCompletionOption.ResponseContentRead, token);
        }

    }
}