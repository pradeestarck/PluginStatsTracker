using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using LiteDB;

namespace PluginStatsServer.Stats
{
    public class TrackedPlugin
    {
        public string ID;

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
            Description = section.GetString("description");
            LogoImage = section.GetString("logo-image");
            InfoLink = section.GetString("info-link");
            FDSSection fieldSection = section.GetSection("fields");
            if (fieldSection is not null)
            {
                foreach (string fieldId in fieldSection.GetRootKeys())
                {
                    Fields.Add(fieldId.ToLowerFast(), new TrackedField(fieldId, fieldSection.GetSection(fieldId)));
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
            DatabaseReports.Insert(Current.Report());
        }
    }
}
