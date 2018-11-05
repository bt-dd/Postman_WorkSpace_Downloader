using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;

namespace Postman_Workspace_Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            const string outputFolder = "Output";

            if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Missing API Key Argument");
                Console.ReadLine();
                return;
            }

            var apiKey = args[0];
            var client = new RestClient("https://api.getpostman.com");
            var request = new RestRequest();
            request.AddHeader("X-Api-Key", apiKey);
            request.AddHeader("cache-control", "no-cache");

            //--Recreate Output Directory
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
                
            }
            Directory.CreateDirectory(outputFolder);

            //--Get All Workspaces
            request.Method = Method.GET;
            request.Resource = "/workspaces";
            var response = CallPostman(client, request);

            var workSpacesResponse = JsonConvert.DeserializeObject<WorkSpacesResponse>(response.Content);

            foreach (var workspace in workSpacesResponse.Workspaces)
            {
                request.Resource = $"/workspaces/{workspace.Id}";
                response = CallPostman(client, request);
                var workSpaceResponse = JsonConvert.DeserializeObject<WorkSpaceResponse>(response.Content);

                var workSpaceOutputFolder = $"{outputFolder}/{CleanFileName(workspace.Name)}";

                if (!Directory.Exists(workSpaceOutputFolder))
                {
                    Directory.CreateDirectory(workSpaceOutputFolder);
                }

                //--Dump all collections to workspace folder

                if (workSpaceResponse.Workspace.Collections != null)
                {
                    var workSpaceCollectionsOutputFolder = $"{outputFolder}/{workspace.Name}/Collections";

                    if (!Directory.Exists(workSpaceCollectionsOutputFolder))
                    {
                        Directory.CreateDirectory(workSpaceCollectionsOutputFolder);
                    }

                    foreach (var collection in workSpaceResponse.Workspace.Collections)
                    {
                        request.Resource = $"/collections/{collection.Id}";
                        response = CallPostman(client, request);
                        File.WriteAllText($"{workSpaceCollectionsOutputFolder}/{CleanFileName(collection.Name)}.json", response.Content);
                    }
                }

                //--Dump all environments to workspace folder

                if (workSpaceResponse.Workspace.Environments != null)
                {
                    var workSpaceEnvironmentsOutputFolder = $"{outputFolder}/{workspace.Name}/Environments";

                    if (!Directory.Exists(workSpaceEnvironmentsOutputFolder))
                    {
                        Directory.CreateDirectory(workSpaceEnvironmentsOutputFolder);
                    }

                    foreach (var environment in workSpaceResponse.Workspace.Environments)
                    {
                        request.Resource = $"/environments/{environment.Id}";
                        response = CallPostman(client, request);
                        File.WriteAllText($"{workSpaceEnvironmentsOutputFolder}/{CleanFileName(environment.Name)}.json", response.Content);
                    }
                }
            }
        }

        public static IRestResponse CallPostman(RestClient client, RestRequest request)
        {
            Thread.Sleep(1000);
            return client.Execute(request);
        }

        public static string CleanFileName(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }

        public class WorkSpacesResponse
        {
            [JsonProperty("workspaces")]
            public List<Workspace> Workspaces { get; set; }
        }

        public class BasicWorkspace
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class WorkSpaceResponse
        {
            [JsonProperty("workspace")]
            public Workspace Workspace { get; set; }
        }

        public class Workspace
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("collections")]
            public List<Collection> Collections { get; set; }

            [JsonProperty("environments")]
            public List<Collection> Environments { get; set; }
        }

        public class Collection
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("uid")]
            public string Uid { get; set; }
        }
    }
}
