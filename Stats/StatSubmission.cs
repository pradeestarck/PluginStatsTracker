using System;
using System.Collections.Generic;

namespace PluginStatsServer.Stats
{
    public class StatSubmission
    {
        public StatSubmitter Submitter;

        public TrackedPlugin ForPlugin;

        public DateTimeOffset TimeSubmitted;

        public struct SubmittedValue
        {
            public TrackedField Field;

            public TrackedFieldValue Value;

            public string Raw;
        }

        public Dictionary<string, SubmittedValue> Values;
    }
}
