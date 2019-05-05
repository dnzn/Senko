namespace Senko
{
    using System.Collections.Generic;

    public partial class SonyDevice
    {
        public Action.IRCode IRCode { get; private set; }

        public partial class Action
        {
            public class IRCode
            {
                SonyDevice _sd { get; set; }

                public bool AutoLoad { get; set; } = true;

                public bool SaveToJsonFile { get; set; } = true;

                Dictionary<string, string> _ircode = new Dictionary<string, string>();
            }
        }
    }
}
