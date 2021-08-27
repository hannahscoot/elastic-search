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
            var testcase = new User
            {
                Name = "*",
                Age = 0,
                Address = id
            };
            var searcharr = new List<User>() { testcase }; // Input array of objects with field name and value/query string (or int) currently with a fake class so it works (obviously to be adapted)
            QueryContainer orQuery = null;
            List<QueryContainer> queryContainerList = new List<QueryContainer>();
            foreach (var item in searcharr)
            {
                orQuery = new QueryStringQuery() { Fuzziness = Fuzziness.Auto, Fields = item.Name, Query = item.Address };
                queryContainerList.Add(orQuery);
                orQuery = new QueryStringQuery() { Fields = item.Name, Query = "*" + item.Address + "*", };
                queryContainerList.Add(orQuery);
            }
            var test = new QueryContainerDescriptor<User>().Bool(b => b.Should(queryContainerList.ToArray()));

            var response = new List<User>();
            var scanResults = elasticClient.Search<User>(s => s
                .Index("users")
                .Size(1)
                .Query(q => test)
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

            var itemList = new List<string>() { "people, 0", "person, 2"}; //Example of synonym list returned from db format, except with dummy data relavant to this case

            elasticClient.Indices.Delete("users");

            elasticClient.Indices.Create("users", i => i
                .Settings(s => s
                    .Analysis(a => a
                        .Tokenizers(t => t.NGram("mynGram", ng => new NGramTokenizer { MaxGram = 10, MinGram = 1 }))
                        .Analyzers(an => an.Custom("mynGram", c => c
                            .Tokenizer("mynGram") 
                            .Filters(new List<string> {"synonym"})
                            )
                        )
                        .TokenFilters(tf => tf.Synonym("synonym", sy => sy.Synonyms(itemList).Format(SynonymFormat.Solr).Expand(false)))
                    )
                    .Setting(UpdatableIndexSettings.MaxNGramDiff, 9)
                )
                .Map<User>(
                    m => m.AutoMap()
                        .Properties(p => p
                            .Text(t => t.Name(n => n.Age).Analyzer("mynGram").SearchAnalyzer("mynGram"))
                        )
                )
            );

            elasticClient.IndexMany<User>(items, "users");

            //elasticClient.Bulk(b => b.Index("users").IndexMany(items));
        }
    }
}