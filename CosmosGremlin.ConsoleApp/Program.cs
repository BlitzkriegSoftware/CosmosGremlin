using Microsoft.Azure.Documents.Client;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Newtonsoft.Json;
using Microsoft.Azure.Graphs.Elements;

namespace CosmosGremlin.ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;

            Title();

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.DoHelp)
                {
                    Usage(null, options);
                    exitCode = 1;
                }

                if (string.IsNullOrWhiteSpace(options.Url))
                {
                    Usage("-c Azure URL required", options);
                    exitCode = 2;
                }

                if (string.IsNullOrWhiteSpace(options.AccountKey))
                {
                    Usage("-k Azure Account Key required", options);
                    exitCode = 3;
                }

                if (exitCode <= 0)
                {
                    DoGraphStuff(options).Wait();
                }
            }

            Environment.ExitCode = exitCode;
            return exitCode;
        }

        public static async Task DoGraphStuff(Options options)
        {
            var dbName = "GraphDb3";
            var collectionName = "People";

            using (DocumentClient client = new DocumentClient(
               new Uri(options.Url),
               options.AccountKey,
               new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp }))
            {
                Database db = client.CreateDatabaseIfNotExistsAsync(
                    new Database() { Id = dbName }
                ).Result;

                DocumentCollection graph =  client.CreateDocumentCollectionIfNotExistsAsync(
                        UriFactory.CreateDatabaseUri(dbName),
                        new DocumentCollection    { Id = collectionName },
                        new RequestOptions { OfferThroughput = 1000 }
                ).Result;

                // Azure Cosmos DB supports the Gremlin API for working with Graphs. Gremlin is a functional programming language composed of steps.
                // Here, we run a series of Gremlin queries to show how you can add vertices, edges, modify properties, perform queries and traversals
                // For additional details, see https://aka.ms/gremlin for the complete list of supported Gremlin operators
                Dictionary<string, string> gremlinQueries = new Dictionary<string, string>
                    {
                        { "Cleanup",        "g.V().drop()" },
                        { "AddVertex 1",    "g.addV('person').property('id', 'thomas').property('firstName', 'Thomas').property('age', 44)" },
                        { "AddVertex 2",    "g.addV('person').property('id', 'mary').property('firstName', 'Mary').property('lastName', 'Andersen').property('age', 39)" },
                        { "AddVertex 3",    "g.addV('person').property('id', 'ben').property('firstName', 'Ben').property('lastName', 'Miller')" },
                        { "AddVertex 4",    "g.addV('person').property('id', 'robin').property('firstName', 'Robin').property('lastName', 'Wakefield')" },
                        { "AddEdge 1",      "g.V('thomas').addE('knows').to(g.V('mary'))" },
                        { "AddEdge 2",      "g.V('thomas').addE('knows').to(g.V('ben'))" },
                        { "AddEdge 3",      "g.V('ben').addE('knows').to(g.V('robin'))" },
                        { "UpdateVertex",   "g.V('thomas').property('age', 44)" },
                        { "CountVertices",  "g.V().count()" },
                        { "Filter Range",   "g.V().hasLabel('person').has('age', gt(40))" },
                        { "Project",        "g.V().hasLabel('person').values('firstName')" },
                        { "Sort",           "g.V().hasLabel('person').order().by('firstName', decr)" },
                        { "Traverse",       "g.V('thomas').outE('knows').inV().hasLabel('person')" },
                        { "Traverse 2x",    "g.V('thomas').outE('knows').inV().hasLabel('person').outE('knows').inV().hasLabel('person')" },
                        { "Loop",           "g.V('thomas').repeat(out()).until(has('id', 'robin')).path()" },
                        { "DropEdge",       "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()" },
                        { "CountEdges",     "g.E().count()" },
                        { "DropVertex",     "g.V('thomas').drop()" },
                    };

                foreach (KeyValuePair<string, string> gremlinQuery in gremlinQueries)
                {
                    Console.WriteLine($"Running {gremlinQuery.Key}: {gremlinQuery.Value}");

                    // The CreateGremlinQuery method extensions allow you to execute Gremlin queries and iterate
                    // results asychronously
                    IDocumentQuery<dynamic> query = client.CreateGremlinQuery<dynamic>(graph, gremlinQuery.Value);
                    while (query.HasMoreResults)
                    {
                        foreach (dynamic result in await query.ExecuteNextAsync())
                        {
                            Console.WriteLine($"\t {JsonConvert.SerializeObject(result)}");
                        }
                    }

                    Console.WriteLine();
                }

                // Data is returned in GraphSON format, which be deserialized into a strongly-typed vertex, edge or property class
                // The following snippet shows how to do this
                string gremlin = gremlinQueries["AddVertex 1"];
                Console.WriteLine($"Running Add Vertex with deserialization: {gremlin}");

                IDocumentQuery<Vertex> insertVertex = client.CreateGremlinQuery<Vertex>(graph, gremlinQueries["AddVertex 1"]);
                while (insertVertex.HasMoreResults)
                {
                    foreach (Vertex vertex in await insertVertex.ExecuteNextAsync<Vertex>())
                    {
                        // Since Gremlin is designed for multi-valued properties, the format returns an array. Here we just read
                        // the first value
                        string name = (string)vertex.GetVertexProperties("firstName").First().Value;
                        Console.WriteLine($"\t Id:{vertex.Id}, Name: {name}");
                    }
                }
            }
        }

        static void Title()
        {
            Console.WriteLine("{0} {1}", Lib.AssemblyHelper.GetTitle(), Lib.AssemblyHelper.GetVersion());
        }

        static void Usage(string message, Options options)
        {
            Console.WriteLine("{0}", options.GetUsage());
            if (!string.IsNullOrWhiteSpace(message)) Console.WriteLine(message);
        }

    }
}
