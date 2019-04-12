namespace Remo
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public partial class Konsole
    {
        public static bool Verbose { get; set; } = true;

        public Konsole()
        {
            Color = new Colors(this);
        }
    }
}
