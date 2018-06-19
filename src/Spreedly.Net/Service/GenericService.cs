using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spreedly.Net.Model;

namespace Spreedly.Net.Service
{
    public class GenericService<TClient>
            where TClient : BaseAsyncClient, new()
    {
        private readonly string _rootUrl;
        private readonly JsonSerializer _serializer;
        private readonly UsernamePasswordKeys _securityKeys;

        public GenericService(string rootUrl, string username, string password, JsonSerializer serializer)
        {
            _securityKeys = new UsernamePasswordKeys(username, password);
            _rootUrl = rootUrl.EndsWith("/") ? rootUrl : (rootUrl + "/");
            _serializer = serializer;
        }

        /// <summary>
        /// Make an http call asynchronously and return the result.
        /// </summary>
        public AsyncCallResult<T> Call<T>(Func<TClient, CancellationToken, Task<HttpResponseMessage>> innerCall) where T : class
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            using (var client = new TClient())
            {
                client.Init(_rootUrl, _securityKeys.Credentials);
                using (var task = innerCall(client, token))
                {
                    try
                    {
                        if (task.Wait(10000, token) == false)
                        {
                            if (token.CanBeCanceled)
                            {
                                source.Cancel();
                            }
                            return new AsyncCallResult<T>(AsyncCallFailureReason.TimeOut);
                        }
                    }
                    catch (Exception)
                    {
                        return new AsyncCallResult<T>(AsyncCallFailureReason.FailedConnection);
                    }
                    if (task.Result.IsSuccessStatusCode == false)
                    {
                        return new AsyncCallResult<T>(AsyncCallFailureReason.FailedStatusCode);
                    }

                    var content = task.Result.Content.ReadAsStreamAsync();
                    if (content.Wait(250, token) == false)
                    {
                        if (token.CanBeCanceled)
                        {
                            source.Cancel();
                        }
                        return new AsyncCallResult<T>(AsyncCallFailureReason.TimeOut);
                    }
                    using (var streamReader = new StreamReader(content.Result))
                    {
                        using (var jsonTextReader = new JsonTextReader(streamReader))
                        {
                            var obj = _serializer.Deserialize<T>(jsonTextReader);
                            return new AsyncCallResult<T>(AsyncCallFailureReason.None, obj);
                        }
                    }
                }
            }
        }

    }
}