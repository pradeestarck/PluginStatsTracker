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
using PluginStatsServer.Stats;
using PluginStatsServer.Models;

namespace PluginStatsServer.Controllers
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
            /*
            
        var playerCountData = google.visualization.arrayToDataTable([
          ['Player Count', 'Server Count'],
          ['1',283],['2',113],['3',59],['4',38],['5',35],['6',27],['8',21],['7',19],['9',9],['14',7],['10',7],['12',7],['11',6],['13',6],['20',4],['15',4],['18',3],['80',3],['28',2],['51',2],['22',2],['39',2],['16',2],['21',2],['75',2],['48',2],['30',1],['67',1],['60',1],['63',1],['77',1],['58',1],['49',1],['23',1],['38',1],['69',1],['76',1],['42',1],['62',1],['26',1],['90',1],['19',1],['37',1],['24',1],['79',1],['45',1],['74',1],['59',1],
        ]);
        var playerCountOptions = {
          title: 'Current Player Counts By Server Count (Excluding Zeroes)'
        };
        var versionsOptions = {
          title: 'Versions Used By Server Count'
        };
        var serverVersionsOptions = {
          title: 'Server Versions Used By Server Count'
        };

        var playerCountChart = new google.visualization.PieChart(document.getElementById('piechart_currentplayers'));
        playerCountChart.draw(playerCountData, playerCountOptions);
        var versionsChart = new google.visualization.PieChart(document.getElementById('piechart_versions'));
        versionsChart.draw(versionsData, versionsOptions);
        var serverVersionsChart = new google.visualization.PieChart(document.getElementById('piechart_serverversions'));
        serverVersionsChart.draw(serverVersionsData, serverVersionsOptions);
      }*/
            StringBuilder graphs = new();
            // <div id="piechart_currentplayers" style="width: 1200px; height: 500px;"></div>
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
                    const string style = "backgroundColor: '#222', pieSliceBorderColor: '#222', titleTextStyle: { color: '#FFFFFF' }, legend: { textStyle: { color: '#FFFFFF' } }";
                    graphs.Append($"<h4>{HtmlJsClean(tracked.Display)}</h4>");
                    if (tracked.Type == TrackedFieldType.INTEGER)
                    {
                        graphs.Append($"Total: {field.Total}<br>Average per all servers: {(field.Total / report.Total)}<br>Average per counted entry: {field.Average}");
                    }
                    js.Append($"var options_{tracked.ID} = {{\ntitle: '', {style}\n}};\n");
                    js.Append($"var chart_{tracked.ID} = new google.visualization.PieChart(document.getElementById('chart_{tracked.ID}'));\nchart_{tracked.ID}.draw(data_{tracked.ID}, options_{tracked.ID});\n");
                    graphs.Append($"<div id=\"chart_{tracked.ID}\" class=\"datachart piechart\"></div>\n");
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
