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
        public static Uri DGrepEndpoint = new Uri("https://dgrepv2-frontend-prod.trafficmanager.net");

        //Time
        public static DateTimeOffset endtime = DateTimeOffset.UtcNow;
        public static DateTimeOffset starttime = DateTimeOffset.UtcNow.AddDays(-1);

        //Event Filters
        public const string NamespaceName = "AzureMessagingRuntime";
        public const string DGrepEventName = "DeploymentUpgradeHistory";

        public static string[] EventFilterNames = { "ScaleUnit", "RoleInstance", "NamespaceName", "OperationResult" };
        public static List<string>[] EventFilterValues = { ScaleUnitsQ, RoleInstancesQ, NamespaceNamesQ, OpResultsQ };

        public static List<EventFilter> GetEventFilters()
        {
            var list = new List<EventFilter> { new EventFilter { NamespaceRegex = NamespaceName, NameRegex = DGrepEventName } };
            return list;
        }

        //Identity Filters
        public enum IdentityFilters
        {
            ScaleUnits,
            RoleInstances,
            NamespaceNames,
            OpResults
        }
        protected static List<string> ScaleUnitsQ = new List<string> { "prod-dxb20-401" };
        protected static List<string> RoleInstancesQ = new List<string> { "SBSQT.17" };
        protected static List<string> NamespaceNamesQ = new List<string> { "vienna-uaenorth-index" };
        protected static List<string> OpResultsQ = new List<string> { "InternalServerError" };

        public static string[] IdentityFilterNames = { "ScaleUnit", "RoleInstance", "NamespaceName", "OperationResult" };
        public static List<string>[] IdentityFilterValues = { ScaleUnitsQ, RoleInstancesQ, NamespaceNamesQ, OpResultsQ };

        public static Dictionary<string, List<string>> GetIdentityFilters()
        {   
            var dict = new Dictionary<string, List<string>>();
            foreach(int filter in Enum.GetValues(typeof(IdentityFilters)))
            {
                dict[ IdentityFilterNames[filter] ] = IdentityFilterValues[filter];
            }
            return dict;
        }

        

    }
}
