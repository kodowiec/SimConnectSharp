namespace SimConnectSharp
{
    public class ConnectionInfo
    {
        public bool Connected { get; set; } = false;
        public bool IsPaused { get; set; } = false; 
        public string ConnectionName { get; set; }

        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string AppBuild { get; set; }

        public string SimConnectVersion {  get; set; }
        public string SimConnectBuild { get; set; }
    }
}
