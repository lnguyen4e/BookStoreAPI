using BookStore_UI.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Threading.Tasks;

namespace BookStore_UI.Service
{
    public class BaseRepository<T> : IBaseRepository<T> where T: class
    {
        private readonly IHttpClientFactory _client;

        public BaseRepository(IHttpClientFactory client)
        {
            _client = client;
        }
        public async Task<bool> Create(string url, T obj)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if(request == null)
            {
                return false;
            }
            request.Content = new StringContent(JsonConvert.SerializeObject(obj));

            var client = _client.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return true;
            }
            return false;
        }

        public Task<T> Delete(string url, int id)
        {
            throw new NotImplementedException();
        }

        public Task<T> Get(string url, int id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<T>> Get(string url)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(string url, T obj)
        {
            throw new NotImplementedException();
        }
    }
}
