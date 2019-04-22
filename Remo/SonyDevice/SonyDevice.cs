﻿namespace Remo
{
    using System.Collections.Generic;
    using System.IO;
    using static Remo.Global;
    using static Remo.Konsole;

    public partial class SonyDevice
    {
        public string ID { get; private set; } = "";

        public string DefaultFilePath { get; set; } = "";

        string IPAddress { get; set; } = "";

        string AuthPSK { get; set; } = "";

        Apps App;

        public SonyDevice()
        {
            Initialize();
        }

        public SonyDevice(string name)
        {
            ID = name;
            DefaultFilePath = @"C:\Users\Danzen Binos\OneDrive\remo\";

            Initialize();
        }

        void Initialize()
        {
            Info = new Information(this, @"C:\Users\Danzen Binos\OneDrive\remo\hub.deviceinfo.json");
            Alias = new Action.Alias(this);
        }
        
        class REST
        {

        }

        class Apps
        {
            public Dictionary<string, Dictionary<string, string>> List { get; private set; }
        }   
    }
}