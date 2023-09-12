using ICM_hackathon_namespace;
using Microsoft.Azure.Monitoring.DGrep.DataContracts.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICM_hackathon_namespace
{
    public class Config
    {   
        //Endpoints
        public static Uri MDSEndpoint = new Uri("https://production.diagnostics.monitoring.core.windows.net/");
        public static Uri DGrepEndpoint = new Uri("https://dgrepv2-frontend-prod.trafficmanager.net/");

        //Time
        public static DateTimeOffset endtime = DateTimeOffset.UtcNow;
        public static DateTimeOffset starttime = DateTimeOffset.UtcNow.AddDays(-5);

        //Event Filters
        public const string NamespaceName = "AzureMessagingRuntime";
        public const string DGrepEventName = "OperationQoSEvents";

        public static List<EventFilter> GetEventFilters()
        {
            var list = new List<EventFilter> { new EventFilter { NamespaceRegex = NamespaceName, NameRegex = DGrepEventName } };
            return list;
        }

        //Identity Filters
        public enum IdentityFilter
        {
            ScaleUnits,
            Roles,
            RoleInstances,
            NamespaceNames,
            OpResults
        }
        protected static List<string> ScaleUnitsQ = new List<string> { "PROD-BY-V51011" };
        protected static List<string> RoleQ = new List<string> { "Backend", "SBSBE" };
        protected static List<string> RoleInstancesQ = new List<string> { "SBSFE.3", "SBSFE.9" };
        protected static List<string> NamespaceNamesQ = new List<string> { "pdakswus01sb" };
        protected static List<string> OpResultsQ = new List<string> { "InternalServerError" };

        public static string[] IdentityFilterNames = { "ScaleUnit", "Role", "RoleInstance", "NamespaceName", "OperationResult" };
        public static List<string>[] IdentityFilterValues = { ScaleUnitsQ, RoleQ, RoleInstancesQ, NamespaceNamesQ, OpResultsQ };

        public static IdentityFilter[] IdentityQueries = {
              IdentityFilter.ScaleUnits, 
              IdentityFilter.Roles,
              //IdentityFilter.RoleInstances, 
              //IdentityFilter.NamespaceNames,
              IdentityFilter.OpResults,
            };
        public static Dictionary<string, List<string>> GetIdentityFilters()
        {   
            var dict = new Dictionary<string, List<string>>();
            foreach(int filter in IdentityQueries)
            {
                dict[ IdentityFilterNames[filter] ] = IdentityFilterValues[filter];
            }
            return dict;
        }


        //InternalServerErrorNodes
        public const int NodesCount = 5;
        public const int ErrorNodeCountFactor = 2;  


    }
}
