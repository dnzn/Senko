namespace Remo
{
    using System.Collections.Generic;
    using System.IO;
    using static Remo.Global;
    using static Remo.Konsole;

    public partial class SonyDevice
    {
        public string Name { get; private set; } = "";

        public string DefaultFilePath { get; set; } = "";

        string IPAddress { get; set; } = "";

        string AuthPSK { get; set; } = "";

        IRCodes IRCode;

        Apps App;

        public SonyDevice()
        {
            Initialize();
        }

        public SonyDevice(string name)
        {
            Name = name;
            DefaultFilePath = @"C:\Users\Danzen Binos\OneDrive\remo\";

            Initialize();
        }

        void Initialize()
        {
            Alias = new CommandAlias(this);
        }
        
        class REST
        {

        }

        class IRCodes
        {
            public Dictionary<string, string> List { get; private set; }
        }

        class Apps
        {
            public Dictionary<string, Dictionary<string, string>> List { get; private set; }
        }        

        public class Info 
        {
            public string Product { get; private set; }

            public string Region { get; private set; }

            public string Language { get; private set; }

            public string Model { get; private set; }

            public string Serial { get; private set; }

            public string MacAddress { get; private set; }

            public string Name { get; private set; }

            public string Generation { get; private set; }

            public string Area { get; private set; }

            public string CID { get; private set; }

            Dictionary<string, string> _keys = new Dictionary<string, string>()
            {
                { "product", "Product" },
                { "region", "Region" },
                { "language", "Language" },
                { "model", "Model" },
                { "serial", "Serial" },
                { "macAddr", "MacAddress" },
                { "name", "Name" },
                { "generation", "Generation" },
                { "area", "Area" },
                { "cid", "CID" }
            };

            public Info(string file)
            {
                // This is temporary. This method will normally access the JSON stream direct from the device and not from a file
                if (File.Exists(file))
                {
                    string json = File.ReadAllText(file);

                    Parse(json);
                }
            }

            void Parse(string file)
            {
                JSON json = new JSON(file);

                foreach (KeyValuePair<string, string> k in _keys)
                {
                    foreach (Dictionary<string, string> lexicon in json.Lexicon)
                    {
                        if (lexicon.ContainsKey(k.Key))
                        {
                            typeof(Info).GetProperty(k.Value).SetValue(this, lexicon[k.Key]);
                        }
                    }
                }
            }

            void ParseIRCodes()
            {

            }
        }
    }
}
