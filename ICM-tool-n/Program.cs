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
    using Microsoft.Cloud.Metrics.Client.MetricsStreaming;

    internal class Program
    {
        public static bool CheckErrorFactor(List<Tuple<string, int>> Nodes, int Node1, int Node2)
        {
            if(Nodes[Node1].Item2 - Nodes[Node2].Item2 > Config.ErrorNodeCountFactor * Nodes[Node2].Item2)
            {
                return true;
            }
            return false;
        }

        //Count defines distinct NodeNames/RoleIstances to be returned
        private static List<Tuple<string, int>> getNodes(Dictionary<string, object>[] Data, int Count)
        {
            List<Tuple<string, int>> Nodes = new List<Tuple<string, int>> { };
            for (int i = 0; i < Count && i < Data.GetLength(0); i++)
            {
                var row = Data[i].ToArray();
                var NodeName = row[0].Value.ToString();
                var NodeCount = int.Parse(row[1].Value.ToString());
                Nodes.Add(new Tuple<string, int>(NodeName, NodeCount));
            }
            return Nodes;
        }

        private static List<Tuple<string, int>> getTopISENodes(Dictionary<string, object>[] Data)
        {
            List<Tuple<string, int>> AllNodes = getNodes(Data, Config.NodesCount), Nodes = new List<Tuple<string, int>> { };
            if (Data.GetLength(0) == 1)
            {
                Nodes.Add(AllNodes[0]);
                return Nodes;
            }
            for (int i = 0; i < Config.NodesCount && i < Data.GetLength(0); i++)
            {
                Nodes.Add(AllNodes[i]);
                if(i < Data.GetLength(0) - 1 && i < Config.NodesCount - 1)
                {
                    if (CheckErrorFactor(AllNodes, i, i + 1)) break;
                }
            }

            return Nodes;
        }

        static async Task Main(string[] args)
        {
            var input = new QueryInput
            {
                MdsEndpoint = Config.MDSEndpoint,
                EventFilters = Config.GetEventFilters(),
                IdentityColumns = Config.GetIdentityFilters(),
                StartTime = Config.starttime,
                EndTime = Config.endtime
            };

            RowSetResult result;
            // Using user based authentication
            using (var client = new DGrepUserAuthClient(Config.DGrepEndpoint))
            {
                result = await client.GetRowSetResultAsync(input, "groupby RoleInstance let Count = Count() orderby Count desc", new CancellationToken());
                var status = result.QueryStatus.Status;
            }
            Dictionary<string, object>[] RetrievedData = result.RowSet.Rows.ToArray();

            List<Tuple<string, int>> ISENodes = getTopISENodes(RetrievedData);
            Console.WriteLine("Following Backend Nodes are showing high Internal server errors: ");
            foreach(var Node in ISENodes)
            {
                Console.WriteLine("{0} has {1} ISEs reported ", Node.Item1, Node.Item2);
            }

            
            Dictionary<string,int> NodeErrors = new Dictionary<string,int>();


        }
    }
}

