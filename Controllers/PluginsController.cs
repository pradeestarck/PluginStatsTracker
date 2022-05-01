using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Html;
using PluginStatsTracker.Stats;
using PluginStatsTracker.Models;

namespace PluginStatsTracker.Controllers
{
    public class PluginsController : Controller
    {
        public IActionResult ViewPlugin()
        {
            if (!Request.HttpContext.Items.TryGetValue("plugin_id", out object pluginIdObject) || pluginIdObject is not string pluginId)
            {
                return Redirect("/Error/Error404");
            }
            if (!StatsServer.TrackedPlugins.TryGetValue(pluginId.ToLowerFast(), out TrackedPlugin plugin))
            {
                return Redirect("/Error/Error404");
            }
            StatReport report = plugin.LastReport();
            StringBuilder js = new();
            StringBuilder graphs = new();
            foreach (StatReport.StatReportField field in report.Fields)
            {
                if (plugin.Fields.TryGetValue(field.FieldID, out TrackedField tracked))
                {
                    js.Append($"var data_{tracked.ID} = google.visualization.arrayToDataTable([\n['Value', 'Count'],\n");
                    foreach (StatReport.StatReportFieldValue value in field.Values)
                    {
                        js.Append($"['{HtmlJsClean(value.Value)}',{value.Count}],");
                    }
                    js.Append("\n]);\n");
                    const string style = "backgroundColor: '#222222', pieSliceBorderColor: '#222222', titleTextStyle: { color: '#FFFFFF' }, legend: { textStyle: { color: '#FFFFFF' } }";
                    graphs.Append($"<div class=\"datachart_box\"><h4>{HtmlJsClean(tracked.Display)}</h4>");
                    if (tracked.Type == TrackedFieldType.INTEGER)
                    {
                        graphs.Append($"Total: {field.Total}<br>Average per all servers: {(field.Total / report.Total):0.##}<br>Average per counted entry: {field.Average:0.##}");
                    }
                    js.Append($"var options_{tracked.ID} = {{\ntitle: '', {style}\n}};\n");
                    js.Append($"var chart_{tracked.ID} = new google.visualization.PieChart(document.getElementById('chart_{tracked.ID}'));\nchart_{tracked.ID}.draw(data_{tracked.ID}, options_{tracked.ID});\n");
                    graphs.Append($"<div id=\"chart_{tracked.ID}\" class=\"datachart piechart\"></div></div>\n");
                }
            }
            return View(new PluginsModel() { Plugin = plugin, Report = report, JS = new HtmlString(js.ToString()), Graphs = new HtmlString(graphs.ToString()) });
        }

        public static string HtmlJsClean(string input)
        {
            return input.Replace("&", "&amp;").Replace("'", "&#39;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("\\", "&#92;");
        }
    }
}
