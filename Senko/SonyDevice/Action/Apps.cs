namespace Senko
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public partial class SonyDevice
    {
        public Action.Apps Apps { get; private set; }

        public partial class Action
        {
            public class Apps
            {
                class Parsed
                {
                    public Result[] result { get; set; }

                    public class Result
                    {
                        public string title { get; set; }
                        public string uri { get; set; }
                        //public string icon { get; set; }
                    }

                    public Dictionary<string, string> ToDictionary()
                    {
                        return result.ToDictionary(x => x.title, x => x.uri);
                    }
                }

                SonyDevice Parent { get; set; }

                public bool AutoLoad { get; set; } = true;

                public bool SaveToJsonFile { get; set; } = true;

                public Dictionary<string, string> List { get; private set; } = new Dictionary<string, string>();

                public Apps(SonyDevice parent, string file)
                {
                    Parent = parent;

                    if (File.Exists(file))
                    {
                        string json = File.ReadAllText(file);

                        Parse(json);
                    }
                }

                bool Parse(string file)
                {
                    List = JsonConvert.DeserializeObject<Parsed>(file).ToDictionary();

                    return true;
                }
            }
        }
    }
}
