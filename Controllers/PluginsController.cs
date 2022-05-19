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
            return View(plugin.GetPageReportData());
        }
    }
}
