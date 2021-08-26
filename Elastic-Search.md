# ElasticSearch

## Setup/Configuration

In order to use Elastic Search, you first have to create an instance, using a docker container. 
If you don't already have Docker configured, follow these instructions below: 

_(If you do have Docker already set up, continue reading after the series of following bullet points)_

 - First you need to set up a Linux env for docker to work properly : Follow these instructions provided by microsoft to set it all up properly, found [here][linux]. At step 6, the recommended distribution for this case is Ubuntu.
 - Next once that is all configured, you need to install Docker, found [here][docker].

 After both these steps, I recommend you run through Docker's quick start guide to check it has all been set-up properly before continuing onto the next setup steps.

 To find the quick-start guide, open docker for desktop then locate the docker icon in the bottom left hand corner of the screen near the sound/wifi icons. Right-click it and in that menu you should see an option for "quick start guide".

To create a local instance of ElasticSearch you follow these steps:

- First visit their site to see what the latest working version of ElasticSearch is, found [here][elasticSearch]. Make a note of the version.

- Next, open up a terminal and enter the following command:
 `docker run -d --name [container name] -v [volume name]:/usr/share/elasticsearch/data -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:[version number]`. 

    Subsituting [container name], [volume name], [version number] with the relavant information.
    
    - [container name] is what you want the container containing your instance of ElasticSearch to be called. If you want to add it to a pre-existing container, then replace with that respective container's name. Otherwise, a good recommendation is `ElasticSearch`.
    - [volume name] is what you want the volume containing your instance of ElasticSearch to be called. If you are using a pre-existing container, ensure this is uniquely named otherwise you may lose whatever is currently stored in that volume. Again, a good recommendation is `ElasticSearch`.
    - [version number] is where you enter the version of ElasticSearch you want to be working with. If you want the latest version, recall the version number/reference in the ealier step. At time of writing the current version is `7.14.0`.

- After letting that command run, you should be able to view the result. In Docker, you should see a container with a running status, and you should be able to select open in browser. 

    Or, alternatively you can visit http://localhost:9200/ to see the instance. 

    If successfull you should see something like the following:

    ```
    {
      "name" : "37554f5ccef7",
        "cluster_name" : "docker-cluster",
        "cluster_uuid" : "wXRjh_qASCGLP8tAHIvTJQ",
        "version" : {
            "number" : "7.13.4",
            "build_flavor" : "default",
            "build_type" : "docker",
            "build_hash" : "c5f60e894ca0c61cdbae4f5a686d9f08bcefc942",
            "build_date" : "2021-07-14T18:33:36.673943207Z",
            "build_snapshot" : false,
            "lucene_version" : "8.8.2",
            "minimum_wire_compatibility_version" : "6.8.0",
            "minimum_index_compatibility_version" : "6.0.0-beta1"
        },
        "tagline" : "You Know, for Search"
    }

Now you have a running instance of ElasticSearch!

## First Run

In order to understand how to work with ElasticSearch, we shall first break down how it works and give a walkthrough/demo of a super simple usecase.

ElasticSearch stores the data in a container. Each group of data is stored as an __index__, the database equivalent of a table. The index is broken down into __documents__, each being the database equivalent of a tuple (or one entry of data). Each documents is broken down into __fields__, which has a value assigned to each one, much like the database equivalent of attributes.

When performing a search, you are querying the indexes - much like querying a database - isn't this so simple!

Now to get your head round how exactly to make use of this, we shall provide an example. 

- We have a dataset `Users`, containing multiple objects of type `User`. The object `User` is made up of 3 fields; `name`, `age` and `address` to keep it simple.

- Here I shall provide a small sample dataset of the aforementioned schema:

        {
            {
                "name": "User1",
                "age":"30",
                "address":"123 Street"
            },
            {
                "name": "User2",
                "age":"32",
                "address":"456 Street"
            },
            {
                "name": "User3",
                "age":"34",
                "address":"789 Street"
            }
        }

Now, to demostrate how we can use and search this data using ElasticSearch, first I recommend you install a HTTP traffic listener/requester such as [Fiddler][fiddler]. This enables you to send requests easily to the ElasticSearch container, and see the response all in one place. _I will discuss other ways of accessing ElasticSearch later on._

This section is if you have Fiddler or equivalent program running, otherwise skip ahead.

- When you load up Fiddler you will see a tab labelled "Composer", this is where you can write and execute your HTTP requests to the ElasticSearch.
- Pick the Parsed tab, then pick you HTTP method in this case we will pick POST, then enter your address of your ElasticSearch container, the standard is `http://localhost:9200/`. 
- There are many approaches to creating an index, we will use a all-in-one method, which adds our sample data above into the index at the same time. This method is called __bulk__ - rather self-explanitory, it adds multiple documents to an index at one time, with a caviat that if the index doesn't exist it will create it first.
- Using our sample data, we first change the URL from `http://localhost:9200/` to `http://localhost:9200/[index]/_bulk` where in this case, `[index]` is replaced by `users`. 
- In the box labeled "Request Body", paste the following: 

        {"index": {}}
        {"name": "User1","age":"30","address":"123 Street"}
        {"index": {}}
        {"name": "User2","age":"32","address":"456 Street"}
        {"index": {}}
        {"name": "User3","age":"34","address":"789 Street"}

    (The `{"index": {}}` is required for the bulk command.)  

- Then in the box above, ensure you have `Content-Type: application/json`.
- Now you can excute your request, and if it has succeeded you should see a HTTP request highlighted in bold black on the left-hand side. Double click on it to see more details, it should under the JSON header provide you with the index and a detailed list of all the documents you also just added to it.

## Searching using Fiddler (or other API service)
- Another way to check it has succeeded is to perform our first 'search' command. Empty out the HTTP request boxes, and pick "GET", and change the url to `http://localhost:9200/[index]/_search` where in this case, `[index]` is replaced by `users`. 

- Now you can excute your request, and if it has succeeded you should see a HTTP request highlighted in bold black on the left-hand side. Double click on it to see more details. You should see each entry you added above embedded in a block of information as follows, the data is stored in the `_source`:

        {   
            "_index":"users",
            "_type":"_doc",
            "_id":"qpzMWXsBm9zkgtrO3lBY",
            "_score":1.0,
            "_source":   {
                "name": "User1",
                "age":"30",
                "address":"123 Street"
            }
        }

## Searching using your Browser

- Navigate in your browser URL to `http://localhost:9200/[index]/_search` where in this case, `[index]` is replaced by `users`. You should be met with a string of data, 

        {"took":1,"timed_out":false,"_shards":{"total":1,"successful":1,"skipped":0,"failed":0},"hits":{"total":{"value":15,"relation":"eq"},"max_score":1.0,"hits":[{"_index":"users","_type":"_doc","_id":"qpzMWXsBm9zkgtrO3lBY","_score":1.0,"_source":{"name": "User1","age":"30","address":"123 Street"}} etc

- To do any query, you add `?q=` to the end of the above browser URL to start the query. One of the most basic ones for our sample data is search a specific field for a specific term `?q=[fieldname]:[search entry]` an example as follows;
        
        localhost:9200/users/_search?q=name:User1


[linux]:https://docs.microsoft.com/en-us/windows/wsl/install-win10
[docker]:https://docs.docker.com/docker-for-windows/install/
[elasticSearch]:https://www.elastic.co/downloads/elasticsearch
[fiddler]:https://www.telerik.com/download/fiddler