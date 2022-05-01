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
using PluginStatsServer.Stats;

namespace PluginStatsServer.Controllers
{
    public class StatsController : Controller
    {
        public IActionResult Submit()
        {
            if (Request.Method != "POST")
            {
                return Redirect("/Error/Error404");
            }
            if (!Request.Form.TryGetValue("postid", out StringValues postIdValue) || postIdValue.Count != 1 || postIdValue[0] != "pluginstats"
                || !Request.Form.TryGetValue("plugin", out StringValues pluginId) || pluginId.Count != 1
                || !Request.Form.TryGetValue("differentiator", out StringValues differentiatorValue) || differentiatorValue.Count != 1)
            {
                return StatusCode(400, "Invalid post request\n");
            }
            if (!StatsServer.TrackedPlugins.TryGetValue(pluginId[0].ToLowerFast(), out TrackedPlugin plugin))
            {
                return StatusCode(400, "Unknown plugin id\n");
            }
            string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            if (StatsServer.TrustXForwardedFor && Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardHeader))
            {
                ip += "/X-Forwarded-For: " + string.Join(" / ", forwardHeader);
            }
            StatSubmission submission = new()
            {
                Submitter = new StatSubmitter(ip, differentiatorValue[0]),
                ForPlugin = plugin
            };
            foreach (string field in Request.Form.Keys.Where(f => f.StartsWith("pl_")))
            {
                string fieldId = field.ToLowerFast()["pl_".Length..];
                if (plugin.Fields.TryGetValue(fieldId, out TrackedField tracked))
                {
                    StatSubmission.SubmittedValue? val = tracked.GetValueFor(Request.Form[field]);
                    if (val.HasValue)
                    {
                        submission.Values[tracked.ID] = val.Value;
                    }
                }
            }
            plugin.Current.Submit(submission);
            return Ok("Submitted\n");
        }
    }
}
