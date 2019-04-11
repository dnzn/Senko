namespace Remo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using static Konsole;
    using static Global;

    public partial class SonyDevice
    {
        public CommandAlias Alias { get; private set; }

        bool _aliasToJsonFile { get; set; }

        bool _aliasAutoLoad { get; set; }

        Dictionary<string, string> _alias { get; set; } = new Dictionary<string, string>();

        public class CommandAlias
        {
            SonyDevice sd;

            string filepath;

            public bool AutoLoad
            {
                get { return sd._aliasAutoLoad; }
                set { sd._aliasAutoLoad = value; }
            }

            public bool SaveToJsonFile
            {
                get { return sd._aliasToJsonFile; }
                set { sd._aliasToJsonFile = value; }
            }

            public CommandAlias(SonyDevice instance, bool autoLoad = true)
            {
                sd = instance;

                AutoLoad = autoLoad;
            }

            void AutoLoadFile()
            {
                string defaultAlias = sd.DefaultFilePath + sd.Name + ".alias";
                string defaultJson = defaultAlias + ".json";

                if (File.Exists(defaultJson))
                {
                    Add(defaultJson);
                }
                else if (File.Exists(defaultAlias))
                {
                    Add(defaultAlias);
                }
                else
                {
                    Kon.WriteLine("Alias Autoload did not find any expected files in the directory specified.\n{0}", sd.DefaultFilePath);
                }
            }

            public void Add(string file)
            {
                filepath = file;

                Dictionary<string, string> Parsed = new Dictionary<string, string>(); 

                if (File.Exists(file))
                {
                    try
                    {
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file)).ToList().ForEach(x => Add(x.Key, x.Value, false));
                    }
                    catch
                    {
                        string[] lines = File.ReadAllLines(file);
                        int duplicates = 0;

                        foreach (string line in lines)
                        {
                            string[] set = line.Split('=');
                            string name = set[0].Trim();

                            foreach (string alias in set[1].Split(','))
                            {
                                if (!Parsed.ContainsKey(alias.Trim().Standardize()))
                                {
                                    Parsed.Add(alias.Standardize(), name);
                                }
                                else
                                {
                                    duplicates++;
                                }
                            }
                        }

                        if (duplicates == 0)
                        {
                            Parsed.ToList().ForEach(x => Add(x.Key, x.Value, false));

                            WriteJsonFile(sd._alias);
                        }
                        else
                        {
                            Kon.WriteLine(Prefix.Prompt, "{0} duplicate aliases have been found. The process has been aborted.\nPlease check the alias file and try again.\n{1}", duplicates, filepath);
                        }
                    }                    
                }
            }

            public void Add(string alias, string name, bool writeFile = true)
            {
                if (!sd._alias.ContainsKey(alias.Standardize()))
                {
                    sd._alias.Add(alias.Standardize(), name.Standardize());

                    if (writeFile)
                    {
                        WriteJsonFile(sd._alias);
                    }
                }
                else
                {
                    Kon.WriteLine("{0} already exists as a key. The process has been aborted.", alias.Standardize());
                }
            }

            void WriteJsonFile(Dictionary<string, string> aliasList)
            {
                if (SaveToJsonFile)
                {
                    string json = JsonConvert.SerializeObject(sd._alias, Formatting.Indented);

                    File.WriteAllText(sd.DefaultFilePath + sd.Name + ".alias.json", json);
                }
            }

            public bool ContainsKey(string key)
            {
                return sd._alias.ContainsKey(key.Standardize());
            }

            public bool ContainsValue(string value)
            {
                return sd._alias.ContainsValue(value);
            }

            public string GetValue(string key)
            {
                if (sd._alias.ContainsKey(key.Standardize()))
                {
                    return sd._alias[key.Standardize()];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
