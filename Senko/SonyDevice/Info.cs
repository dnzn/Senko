namespace Senko
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    public partial class SonyDevice
    {
        public Information Info { get; private set; }

        public class Information
        {
            class Parsed
            {
                public Result[] result { get; set; }

                public class Result
                {
                    public string product { get; set; }
                    public string region { get; set; }
                    public string language { get; set; }
                    public string model { get; set; }
                    public string serial { get; set; }
                    public string macAddr { get; set; }
                    public string name { get; set; }
                    public string generation { get; set; }
                    public string area { get; set; }
                    public string cid { get; set; }
                }

                public Dictionary<string, string> ToDictionary()
                {
                    return result[0].GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(result[0])?.ToString() ?? "");
                }
            }

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

            public Information(SonyDevice instance, string file)
            {
                // This is temporary. This method will normally access the JSON stream direct from the device and not from a file
                if (File.Exists(file))
                {
                    string json = File.ReadAllText(file);

                    Parse(json);
                }
            }

            bool Parse(string file)
            {
                Dictionary<string, string> parsed = JsonConvert.DeserializeObject<Parsed>(file).ToDictionary();

                for (int i = 0; i < parsed.Count; i++)
                {
                    typeof(Information).GetProperty(this.GetType().GetProperties()[i].Name).SetValue(this, parsed[parsed.Keys.ToArray()[i]]);
                }

                return true;
            }
        }
    }
}
