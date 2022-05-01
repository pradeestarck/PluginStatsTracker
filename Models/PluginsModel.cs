using System;
using Microsoft.AspNetCore.Html;
using PluginStatsServer.Stats;

namespace PluginStatsServer.Models
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
            return time.Length == 10 ? $"{time[0..4]}/{time[5..6]}/{time[7..8]} {time[9..10]}:00 UTC" : "(UNKNOWN)";
        }
    }
}
