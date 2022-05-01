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
    public class TrackedPlugin
    {
        public string ID;

        public string Name;

        public string Description;

        public string LogoImage;

        public string InfoLink;

        public Dictionary<string, TrackedField> Fields = new();

        public CurrentPluginStats Current;

        public ILiteCollection<StatReport> DatabaseReports;

        public TrackedPlugin(string _id, ILiteCollection<StatReport> reports, FDSSection section)
        {
            ID = _id;
            DatabaseReports = reports;
            Name = section.GetString("name");
            Description = section.GetString("description");
            LogoImage = section.GetString("logo-image");
            InfoLink = section.GetString("info-link");
            FDSSection fieldSection = section.GetSection("fields");
            if (fieldSection is not null)
            {
                foreach (string fieldId in fieldSection.GetRootKeys())
                {
                    string field = fieldId.ToLowerFast();
                    Fields.Add(field, new TrackedField(field, fieldSection.GetSection(fieldId)));
                }
            }
            Current = new CurrentPluginStats() { ForPlugin = this };
        }

        public void MakeReport(int now)
        {
            if (DatabaseReports.FindById(now) is not null)
            {
                return;
            }
            Current.Clean();
            DatabaseReports.Insert(Current.Report());
        }

        public StatReport LastReport()
        {
            int now = StatsServer.GetCurrentTimeID();
            StatReport result = DatabaseReports.FindById(now);
            if (result is not null)
            {
                return result;
            }
            for (int i = 0; i < 48; i++)
            {
                result = DatabaseReports.FindById(now - i);
                if (result is not null)
                {
                    return result;
                }
            }
            return new StatReport() { ForPluginID = ID, Fields = Array.Empty<StatReport.StatReportField>() };
        }
    }
}
