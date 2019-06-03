namespace Senko
{
    public partial class SonyDevice
    {
        public string ID { get; private set; } = "";

        public string DefaultFilePath { get; set; } = "";

        string IPAddress { get; set; } = "";

        string AuthPSK { get; set; } = "";

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
            Command = new Action.Command(this, @"C:\Users\Danzen Binos\OneDrive\remo\hub.ircodelist.json");
            Apps = new Action.Apps(this, @"C:\Users\Danzen Binos\OneDrive\remo\hub.applist.json");
            Alias = new Action.Alias(this);
        }
        
        class REST
        {

        }
    }
}
