namespace WaterThePlant
{
    public class PostRequestData
    {
        public string? moisturelevel { get; set; }

        public bool? automode { get; set; }

        public string? motorstate { get; set; }

        public bool getconfig { get; set; }

        public bool reportMoistureLevel { get; set; }

        public bool getHistory { get; set; }
    }

}
