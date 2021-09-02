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
        [HttpGet("post")]
        public string Post(string previousInfo, string name)
        { 
            var newUser = new User()
            {
                Name = previousInfo,
                Age = 45, 
                Address = name
            };

            var bulkIndexer = new BulkDescriptor();
                    bulkIndexer.Update<User>(i => i
                    .DocAsUpsert(true)
                    .Id(newUser.Name)
                    .Doc(newUser)
                    .Index("users"));
                var bulkResponse = elasticClient.Bulk(bulkIndexer);
            return bulkResponse.IsValid.ToString();
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
                    Address = "SWC"
                },
                new User
                {
                    Name = "User2",
                    Age = 32,
                    Address = "bop"
                },
                new User
                {
                    Name = "User3",
                    Age = 34,
                    Address = "cuk"
                }
            };

            var itemList = new List<string>() { "ting, bop", "shes, cuk" }; //Example of synonym list returned from db format, except with dummy data relavant to this case

            elasticClient.Indices.Delete("users");

            elasticClient.Indices.Create("users", i => i
                .Settings(s => s
                    .Analysis(a => a
                        .TokenFilters(tf => tf.Synonym("synonym", sy => sy.Synonyms(itemList).Format(SynonymFormat.Solr).Expand(false)))
                        .Tokenizers(t => t.NGram("mynGram", ng => new NGramTokenizer { MaxGram = 10, MinGram = 1 }))
                        .Analyzers(an => an
                            .Custom("mynGram", c => c
                                .Tokenizer("mynGram")
                                .Filters(filters: new List<string> { "synonym" })
                            )
                            .Custom("other", c => c
                                .Tokenizer("standard")
                                .Filters(filters: new List<string> { "synonym" })
                            )
                        )
                    )
                    .Setting(UpdatableIndexSettings.MaxNGramDiff, 9)
                )
                .Map<User>(
                    m => m.AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Age)
                                .Analyzer("mynGram").SearchAnalyzer("other"))
                            .Text(t => t
                                .Name(n => n.Name)
                                .Name(n => n.Address)
                                .Analyzer("other").SearchAnalyzer("other"))
                        )
                )
            );

            var bulkIndexer = new BulkDescriptor();
            foreach (var document in items)
            {
                bulkIndexer.Index<User>(i => i
                    .Id(document.Name)
                    .Document(document)
                    .Index("users"));
            }
            var bulkResponse = elasticClient.Bulk(bulkIndexer);

            //elasticClient.Bulk(b => b.Index("users").IndexMany(items));
        }
    }
}