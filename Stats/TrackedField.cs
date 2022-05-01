using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace PluginStatsServer.Stats
{
    public class TrackedField
    {
        public string ID;

        public TrackedFieldType Type;

        public string Display;

        public bool Any = false;

        public List<TrackedFieldValue> Values;

        public TrackedField(string _id, FDSSection section)
        {
            ID = _id;
            Type = Enum.Parse<TrackedFieldType>(section.GetString("type"), true);
            Display = section.GetString("display");
            Any = section.GetBool("any", false).Value;
            Values = section.GetString("values", "").SplitFast(',').Select(s => s.Replace(" ", "")).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => new TrackedFieldValue(s)).ToList();
        }

        public StatReport.StatReportField Report(StatSubmission[] stats)
        {
            Dictionary<string, int> counters = new();
            foreach (StatSubmission stat in stats)
            {
                if (stat.Values.TryGetValue(ID, out StatSubmission.SubmittedValue val))
                {
                    if (counters.TryGetValue(val.Value.Text, out int count))
                    {
                        counters[val.Value.Text] = count + 1;
                    }
                    else
                    {
                        counters[val.Value.Text] = 1;
                    }
                }
            }
            return new()
            {
                FieldID = ID,
                Values = counters.Select(pair => new StatReport.StatReportFieldValue() { Value = pair.Key, Count = pair.Value }).ToArray()
            };
        }
    }

    public class TrackedFieldValue
    {
        public string Text;

        public int Min, Max;

        public TrackedFieldValue(string _text)
        {
            Text = _text;
            if (Text.EndsWith('+'))
            {
                Max = int.MaxValue;
                Min = int.Parse(Text[..^1]);
            }
            else if (Text.Contains('-'))
            {
                string before = Text.BeforeAndAfter('-', out string after);
                Min = int.Parse(before);
                Max = int.Parse(after);
            }
            else
            {
                Min = int.Parse(Text);
                Max = Min;
            }
        }

        public bool AppliesTo(int value)
        {
            return value >= Min && value <= Max;
        }
    }
}
