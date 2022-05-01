using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using LiteDB;

namespace PluginStatsTracker.Stats
{
    public class StatReport
    {
        [BsonId]
        public int StatID { get; set; }

        public string ForPluginID { get; set; }

        public StatReportField[] Fields { get; set; }

        public int Total { get; set; }

        public class StatReportField
        {
            public string FieldID { get; set; }

            public float Average { get; set; }

            public float Total { get; set; }

            public int Counted { get; set; }

            public StatReportFieldValue[] Values { get; set; }
        }

        public class StatReportFieldValue
        {
            public string Value { get; set; }

            public int Count { get; set; }
        }
    }
}
