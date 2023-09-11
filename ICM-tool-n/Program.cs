namespace ICM_tool_n
{

    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.DGrep;
    using Microsoft.Cloud.Metrics.Client;
    using Microsoft.Cloud.Metrics.Client.Metrics;
    using Microsoft.Cloud.Metrics.Client.Query;
    using Microsoft.Online.Metrics.Serialization.Configuration;
    using System.Threading;

    internal class Program
    {
        public static string GetScaleUnit(string serviceNamespace, string type)
        {
            if (type == "public")
            {
                Ping myPing = new Ping();
                Console.WriteLine($"{serviceNamespace}.servicebus.windows.net");
                PingReply reply =  myPing.Send($"{serviceNamespace}.servicebus.windows.net", 500);
                return reply.ToString();
            }
            else
            {
                return "x";
            }
        }

        public static Boolean CheckKPIDrop(string scaleUnit)
        {
            // 2 Layers in the query.
            Dictionary<string, string[]> dimensions = new Dictionary<string, string[]>();
            dimensions.Add("ScaleUnit", new string[] { scaleUnit });
          //  dimensions.Add("NamespaceType", new string[] { "Microsoft.EventHub" });
            double[] kpiMetricsRequests = getMetrics(dimensions, "servicebus", "OperationQoSMetrics", "IncomingRequests", 0, 0, "Avg");

            Dictionary<string, string[]> dimensionsFailed = new Dictionary<string, string[]>();
            dimensionsFailed.Add("ScaleUnit", new string[] { scaleUnit });
            dimensionsFailed.Add("OperationResult", new string[] { "InternalServerError" });
          //  dimensionsFailed.Add("NamespaceType", new string[] { "Microsoft.EventHub" });

            double[] kpiMetricsFailed = getMetrics(dimensionsFailed, "servicebus", "OperationQoSMetrics", "IncomingRequests", 0, 0, "Avg");
            double[] kpiMetrics = new double[kpiMetricsFailed.Length];
            for(int i=0; i<kpiMetricsFailed.Length; i++)
            {

                kpiMetrics[i] = Math.Round(((Math.Max((kpiMetricsRequests[i] >= kpiMetricsFailed[i] ? (kpiMetricsRequests[i] - kpiMetricsFailed[i]) : 0) / (Math.Max(kpiMetricsRequests[i], 1)), 1) * 100) * 10000) / 10000);
            }

            Boolean isDrop = false;
            // mathematical check ?

            foreach(double num in kpiMetrics)
            {
                if (num < 96)
                {
                    isDrop = true;
                }
            }

            // anomaly detector check

            return isDrop;


        }

        public async Task<double[]> getMetrics(Dictionary<string, string[]> dimensionList, string monitoringAccount, string metricNamespace, string metricName, int startTime, int endTime, string samplingType )
        {
            var input = new QueryInput
            {
                MdsEndpoint = new Uri("https://production.diagnostics.monitoring.core.windows.net/"),
                EventFilters = new List<EventFilter>
    {
        new EventFilter { NamespaceRegex = "^DGrep$", NameRegex = "^LogTrace$" },
    },
                StartTime = DateTimeOffset.Now.AddMinutes(-30),
                EndTime = DateTimeOffset.Now
            };

            // Using certificate based authentication
            using (var client = new DGrepClient(DGrepEndpoint, ClientCertificate))
            {
                RowSetResult result = await client.GetRowSetResultAsync(input, cancellationToken);
                var firstMessage = result.RowSet.Rows.First()["Message"];
                var status = result.QueryStatus.Status;
            }

            // Using user based authentication
            using (var client = new DGrepUserAuthClient(DGrepEndpoint))
            {
                RowSetResult result = await client.GetRowSetResultAsync(input, cancellationToken);
                var firstMessage = result.RowSet.Rows.First()["Message"];
                var status = result.QueryStatus.Status;
            }

        }

        public Task downloadETLLogs()
        {
            // jarvis actions or sbot
            // get metadata
            // use it to get storage account url
            return new Task(() => { });
        }

        
        static void Main(string[] args)
        {
            string serviceNamespace = "lastmile-un-p01";
            string scaleUnit = GetScaleUnit(serviceNamespace, "public");
            scaleUnit = "prod-dxb20-401";
            Boolean isDrop = CheckKPIDrop(scaleUnit);
            if (isDrop)
            {
                // implies errors: ISE, Storage Exceptions, CPU, Memory
            }
            else
            {
                // implies errors: CE, SB
            }





            

        }
    }
}
