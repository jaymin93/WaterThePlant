namespace WaterThePlant
{
    public class GETRequestData
    {
        public int moisturelevel { get; set; }

        public bool automode { get; set; } = true;

        public int motorstate { get; set; }

        public bool getconfig { get; set; }

        public bool reportMoistureLevel { get; set; }

        public bool getHistory { get; set; }
    }

}
