﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using LiteDB;

namespace PluginStatsServer.Stats
{
    public class StatsServer
    {
        public static FDSSection Config;

        public static string URLBase;

        public static bool TrustXForwardedFor;

        public static Dictionary<string, TrackedPlugin> TrackedPlugins = new();

        public static CancellationTokenSource CancelToken = new();

        public static LiteDatabase Database;

        public static void Load()
        {
            Console.WriteLine("Loading...");
            Directory.CreateDirectory("./data/");
            Database = new LiteDatabase("./data/database.ldb");
            Config = FDSUtility.ReadFile("./config/config.fds");
            URLBase = Config.GetString("url-base");
            TrustXForwardedFor = Config.GetBool("trust-x-forwarded-for", false).Value;
            FDSSection pluginSection = Config.GetSection("plugins");
            foreach (string pluginId in pluginSection.GetRootKeys())
            {
                string id = pluginId.ToLowerFast();
                TrackedPlugins.Add(id, new TrackedPlugin(id, Database.GetCollection<StatReport>($"reports_plugin_{id}"), pluginSection.GetSection(pluginId)));
            }
            Task.Factory.StartNew(IdleLoop);
        }

        public static int GetCurrentTimeID()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return int.Parse(now.ToString("yyyyMMddHH"));
        }

        public static void IdleLoop()
        {
            int lastRecorded = GetCurrentTimeID();
            Console.WriteLine($"Looping started at {lastRecorded}");
            while (!CancelToken.IsCancellationRequested)
            {
                try
                {
                    Task.Delay(TimeSpan.FromMinutes(10)).Wait(CancelToken.Token);
                    int now = GetCurrentTimeID();
                    if (now == lastRecorded)
                    {
                        continue;
                    }
                    if (CancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    lastRecorded = now;
                    foreach (TrackedPlugin plugin in TrackedPlugins.Values)
                    {
                        if (CancelToken.IsCancellationRequested)
                        {
                            return;
                        }
                        Console.WriteLine($"Doing stat gather for {plugin.ID} at {now}");
                        plugin.MakeReport(now);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Idle loop errored: {ex}");
                }
            }
        }

        public static void Close()
        {
            CancelToken.Cancel();
            Console.WriteLine("Performing database shutdown...");
            Database.Dispose();
            Console.WriteLine("Database has been shutdown.");
        }
    }
}
