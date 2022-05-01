using System;
using Microsoft.AspNetCore.Html;
using PluginStatsTracker.Stats;

namespace PluginStatsTracker.Models
{
    public class PluginsModel
    {
        public StatReport Report;

        public TrackedPlugin Plugin;

        public HtmlString JS;

        public HtmlString Graphs;

        public string RenderDate()
        {
            string time = Report.StatID.ToString();
            return time.Length == 10 ? $"{time[0..4]}/{time[4..6]}/{time[6..8]} {time[8..10]}:00 UTC" : "(UNKNOWN)";
        }
    }
}
