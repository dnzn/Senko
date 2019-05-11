namespace Senko
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public partial class SonyDevice
    {
        public Action.IRCode IRCode { get; private set; }

        public partial class Action
        {
            public class IRCode
            {
                class Parsed
                {
                    public Result[] result { get; set; }

                    public class Result
                    {
                        public string name { get; set; }
                        public string value { get; set; }
                    }

                    public Dictionary<string, string> ToDictionary()
                    {
                        return result.ToDictionary(x => x.name, x => x.value);
                    }
                }
                
                SonyDevice This { get; set; }

                public bool AutoLoad { get; set; } = true;

                public bool SaveToJsonFile { get; set; } = true;

                public Dictionary<string, string> Lexicon { get; private set; } = new Dictionary<string, string>();

                public IRCode(SonyDevice instance, string file)
                {
                    This = instance;

                    if (File.Exists(file))
                    {
                        string json = File.ReadAllText(file);

                        Parse(json);
                    }
                }

                bool Parse(string file)
                {
                    Lexicon = JsonConvert.DeserializeObject<Parsed>(file).ToDictionary();

                    return true;
                }
            }
        }
    }
}
