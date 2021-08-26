using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace New_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IElasticClient elasticClient;
        public UsersController(IElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        private static readonly HttpClient client = new HttpClient();

        //GET api/Users
        [HttpGet]
        public List<User> GetAll()
        {
            /*var response = elasticClient.Search<User>(s => s
                .Index("users")
                .Size(10000) //This is the max value allowed (index.max_result_window)
                .Query(q => q.MatchAll()));
            return response.Documents.ToList();*/

            /*var response = new List<User>();
            for (int i=0; i < 4; i++)
            {
                var webRequest = new HttpRequestMessage(HttpMethod.Get, ("http://localhost:9200/users/_search?size=2&from=" + i*2));
                var webResponse = client.Send(webRequest);
                using var reader = new StreamReader(webResponse.Content.ReadAsStream());
                var singleResponse = JObject.Parse(reader.ReadToEnd());
                var attempt = (object)singleResponse.SelectToken("hits.hits");
                Console.WriteLine(attempt);
            }*/


            var SearchTerm = @"{
                                ""query"": {
                                    ""fuzzy"": { 
                                        ""name"": {
                                            ""value"": ""user""
                                        }
                                    }
                                }
                            }";

            SearchRequest searchRequest;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(SearchTerm)))
            {
                searchRequest = elasticClient.SourceSerializer.Deserialize<SearchRequest>(stream);
            }

            ISearchResponse<User> responses;
            responses = elasticClient.Search<User>(searchRequest);
            return responses.Documents.ToList();

            var response = new List<User>();
            var scanResults = elasticClient.Search<User>(s => s
                .Index("users")
                .Size(1)
                .MatchAll()
                .Scroll("10m")
            );
            response = scanResults.Documents.ToList();
            var results = elasticClient.Scroll<User>("10m", scanResults.ScrollId);
            while (results.Documents.Any())
            {
                foreach (var doc in results.Documents)
                {
                    response.Add(doc);
                }
                results = elasticClient.Scroll<User>("10m", results.ScrollId);
            }
            return response;
        }
        // GET api/Users/id
        [HttpGet("{id}")]
        public List<User> Get(string id)
        {
            var webRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:9200/users/_search?q=*"+id.ToLower()+"*"); //searches all fields for terms containing the entered phrase
            /*http://localhost:9200/users/_search?q=name:*2*%20|%20*:usr3~AUTO */ //searches name containing a 2 OR any field containing the usr3 to fuzziness of AUTO ( %20|%20 <=> " | " )

            var responseweb = client.Send(webRequest);
            using var reader = new StreamReader(responseweb.Content.ReadAsStream());
            Console.WriteLine(reader.ReadToEnd());
            return new List<User>() { };
            //var input = id;
            ////A input based search by name
            //var response = elasticClient.Search<User>(s => s
            //    .Index("users")
            //    //.Query(q => q.Term(f => f.Age, 34))); //This searches for a matching value (int)
            //    //.Query(q => q.Match(m => m.Field("name").Query(input)))); //This searched for a matching value
            //    .Query(q => q.Bool(b => b.Should(
            //      //m => m.Wildcard(c => c.Field("age").Value("*" + input.ToLower() + "*")), 
            //      m => m.Wildcard(c => c.Field("name").Value("*"+input.ToLower()+"*")))))); //This searches for values containing the input, on both fields names,age
            //    //.Query(q => q.Bool(b => b.Should(
            //        //s => s.Fuzzy(f => f.Field("name").Value(input)),
            //        //s => s.Fuzzy(f => f.Field("address").Value(input)))))); //This search for values similar to input,  on both fields names,address
            //return response?.Documents.ToList();
        }
        // POST api/Users
        [HttpPost]
        public string Post([FromBody] User value)
        {
            /////// Super Simple Post needs format of {name: "", age: "", address:""}/////// 
            var response = elasticClient.Index<User>(value, x => x.Index("users"));
            return response.Id;
        }
        // DELETE api/Users
        [HttpDelete]
        public bool Delete()
        {
            var response = elasticClient.Indices.Delete("users");
            return response.Acknowledged;
        }

        // POST api/Users/arr   //Dumps entire array of objects into the index
        [HttpGet("arr/test")]
        public void PostArr()
        {
            var items = new[]
            {
                new User
                {
                    Name = "User1",
                    Age = 30,
                    Address = "123 Street"
                },
                new User
                {
                    Name = "User2",
                    Age = 32,
                    Address = "456 Street"
                },
                new User
                {
                    Name = "789 Street",
                    Age = 34,
                    Address = "User3"
                }
            };

            elasticClient.Indices.Delete("users");

            elasticClient.Indices.Create("users");

            elasticClient.IndexMany<User>(items, "users");

            //elasticClient.Bulk(b => b.Index("users").IndexMany(items));
        }
    }
}
