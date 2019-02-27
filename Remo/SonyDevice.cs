using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using static Remo.Global;
using static Remo.Konsole;
using System.Reflection;

namespace Remo
{
    public class SonyDevice
    {
        public string Name { get; private set; }
        public string FilePath { get; set; }

        public class Device
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

            Dictionary<string, string> keys = new Dictionary<string, string>()
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

            public Device(string file)
            {
                if (File.Exists(file))
                {
                    string json = File.ReadAllText(file);

                    Parse(json);
                }
            }

            void Parse(string json)
            {
                JSON parser = new JSON(json);

                foreach (KeyValuePair<string, string> k in keys)
                {
                    if (parser.Lexicon.ContainsKey(k.Key))
                    {
                        typeof(Device).GetProperty(k.Value).SetValue(this, parser.Lexicon[k.Key]);
                    }
                }
            }
        }

        class Alias
        {
            Dictionary<string, string> Lexicon { get; set; }
            
            public Alias(string file)
            {
                if (File.Exists(file))
                {
                    var lines = File.ReadAllLines(file);

                    foreach (var line in lines)
                    {
                        string[] set = line.Split('=');
                        string name = set[0];

                        foreach (string alias in set[1].Split(','))
                        {
                            Lexicon.Add(alias, name);
                        }
                    }

                    Kon.WriteLine(Prefix.Indent, "All aliases have been added to the dictionary successfully!");
                }
                else
                {
                    Kon.WriteLine(Prefix.Indent, "File does not exist. Aborting...");
                }
            }
        }
    }
}