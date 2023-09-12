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
    using ICM_hackathon_namespace;



    internal class Program
    {
        static async Task Main(string[] args)
        {
            var input = new QueryInput
            {
                MdsEndpoint = Config.MDSEndpoint,
                EventFilters = new List<EventFilter>
                {

                    new EventFilter { NamespaceRegex = Config.NamespaceName, NameRegex = Config.DGrepEventName }
                },
                IdentityColumns = Config.GetIdentityFilters(),
                StartTime = Config.starttime,
                EndTime = Config.endtime
            };

            // Using user based authentication
            using (var client = new DGrepUserAuthClient(Config.DGrepEndpoint))
            {
                RowSetResult result = await client.GetRowSetResultAsync(input, new CancellationToken());
                var status = result.QueryStatus.Status;
            }

        }
    }
}

