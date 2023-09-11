namespace ICM_hackathon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Cloud.Metrics.Client;
    using Microsoft.Cloud.Metrics.Client.Metrics;
    using Microsoft.Cloud.Metrics.Client.Query;
    using Microsoft.Online.Metrics.Serialization.Configuration;
    using Microsoft.Azure.Monitoring.DGrep.SDK;
    using Microsoft.Azure.Monitoring.DGrep.DataContracts.External;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;



    internal class Program
    {

        public static string GetScaleUnit(string serviceNamespace, string type)
        {
            if (type == "public")
            {
                Ping myPing = new Ping();
                Console.WriteLine($"{serviceNamespace}.servicebus.windows.net");
                PingReply reply = myPing.Send($"{serviceNamespace}.servicebus.windows.net", 500);
                return reply.ToString();
            }
            else
            {
                return "x";
            }
        }




        public static async Task<double[]> getMetrics(Dictionary<string, string[]> dimensionList, string monitoringAccount, string metricNamespace, string metricName, DateTime startTime, DateTime endTime, SamplingType samplingType)
        {
            var connectionInfo = new ConnectionInfo();
            var reader = new MetricReader(connectionInfo);
            var id = new MetricIdentifier(monitoringAccount, metricNamespace, metricName);



            Console.WriteLine(id.ToString());



            var dimensionFilters = new List<DimensionFilter>();
            foreach (var dimension in dimensionList)
            {
                dimensionFilters.Add(DimensionFilter.CreateIncludeFilter(dimension.Key, dimension.Value));



            }





            IQueryResultListV3 results = reader.GetTimeSeriesAsync(
                id,
                dimensionFilters,
                startTime, endTime,
                new[] { samplingType }
             ).Result;
            double[] metrics = new double[] { };
            foreach (var series in results.Results)
            {
                Console.WriteLine(series.ToString());
                IEnumerable<string> dimensions = series.DimensionList.Select(x => string.Format("[{0}, {1}]", x.Key, x.Value));
                metrics = series.GetTimeSeriesValues(samplingType);





            }




            return metrics;



        }
        class SeriesObj
        {



            public DateTime timestamp;
            public double value;
            public SeriesObj(DateTime timestamp, double value)
            {
                this.timestamp = timestamp;
                this.value = value;
            }





        }



        public Task downloadETLLogs()
        {
            // jarvis actions or sbot
            // get metadata
            // use it to get storage account url
            return new Task(() => { });
        }



        public static async Task<string> checkIfHighIncomingRequests(string scaleUnit)
        {
            Dictionary<string, string[]> dimensionList = new Dictionary<string, string[]>
            {
                { "ScaleUnit", new string[] { scaleUnit } },


            };
            var metrics = await getMetrics(dimensionList, "servicebus", "OperationQoSMetrics", "IncomingRequests", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow, SamplingType.Sum);
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    List<SeriesObj> result = new List<SeriesObj>();
                    for (int i = 0; i < metrics.Length; i++)
                    {
                        DateTime currentDateTime = DateTime.Now;
                        int minutesToSubtract = metrics.Length - i;
                        DateTime timestamp = currentDateTime.AddMinutes(-minutesToSubtract);
                        result.Add(new SeriesObj(timestamp, metrics[i]));
                    }



                    // Serialize the C# object to JSON
                    string jsonData = JsonConvert.SerializeObject(result);



                    // Create a StringContent object with the JSON data
                    var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");



                    // Send a POST request to the specified URL with the JSON data
                    HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:5000/post_example", content);



                    // Check if the response status code indicates success (2xx)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the content of the response as a string
                        string responseBody = await response.Content.ReadAsStringAsync();



                        // Print the response content to the console
                        Console.WriteLine(responseBody);




                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"HttpRequestException: {e.Message}");
                }
            }
            return "";
        }



        static async Task Main(string[] args)
        {
            // If query Geneva Metrics INT environment use : var connectionInfo = new ConnectionInfo(MdmEnvironment.Int);
            var connectionInfo = new ConnectionInfo();
            Dictionary<string, string[]> dimensionList = new Dictionary<string, string[]>
            {
                { "ScaleUnit", new string[] { "prod-dxb20-401" } },


            };
            var metrics = await getMetrics(dimensionList, "servicebus", "OperationQoSMetrics", "IncomingRequests", DateTime.UtcNow.AddHours(-3), DateTime.UtcNow, SamplingType.Sum);



            var reader = new MetricReader(connectionInfo);
            //await checkIfHighIncomingRequests("prod-dxb20-401");
            // Metric name where Account=MetricTeamInternalMetrics, Namespace=PlatformMetrics, Name=\\Memory\\Available MBytes
            /*var id = new MetricIdentifier("servicebus", "OperationQoSMetrics", "Role");

 

            IEnumerable<DimensionFilter> dimensionFilters = new List<DimensionFilter>
            {
                // Filter represents the condition : Region
                DimensionFilter.CreateIncludeFilter("ScaleUnit", "prod-dxb20-401")
            };

 

            // Get SUM and COUNT data for last 10 minutes for Top 10 timeseries keys
            // Where (__Role == Hinting || __Role == MetricsStore) AND (Region == "EAST US")
            // Order by AVERAGE SUM in descending order
            // Filters dimensions set should match a configured pre-aggregate on the metric
            // If you are using custom sampling type, just use new SamplingType({SamplingTypeNameString}) 
            var knownTimeSeriesDefinitions = reader.GetKnownTimeSeriesDefinitionsAsync(
                id,
                dimensionFilters,
                DateTime.UtcNow.AddMinutes(-10),
                DateTime.UtcNow).Result;

 

            var roleIndex = knownTimeSeriesDefinitions.FirstOrDefault();
            Console.WriteLine(roleIndex);

 

            foreach (var value in knownTimeSeriesDefinitions)
            {
                Console.WriteLine("Known timeseries definition: {0}", JsonConvert.SerializeObject(value.DimensionCombination));
            }*/
        }
    }
}