﻿using System;
using System.Collections.Generic;
using LMeter.Helpers;
using Newtonsoft.Json;

namespace LMeter.Config
{
    [JsonObject]
    public class LMeterConfig : IConfigurable, IPluginDisposable
    {
        public bool FirstLoad = true;
        public string Version => Plugin.Version;
        public string Name
        {
            get => "LMeter";
            set { }
        }

        [JsonIgnore]
        private AboutPage AboutPage { get; } = new AboutPage();

        public MeterListConfig MeterList { get; init; }
        public ActConfig ActConfig { get; init; }
        public FontConfig FontConfig { get; init; }

        public LMeterConfig()
        {
            this.MeterList = new MeterListConfig();
            this.ActConfig = new ActConfig();
            this.FontConfig = new FontConfig();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ConfigHelpers.SaveConfig(this);
            }
        }

        public IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.MeterList;
            yield return this.ActConfig;
            yield return this.FontConfig;
            yield return this.AboutPage;
        }

        public void ImportPage(IConfigPage page) { }
    }
}
