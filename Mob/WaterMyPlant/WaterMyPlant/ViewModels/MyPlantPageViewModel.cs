using Microcharts;
using Newtonsoft.Json;
using RestSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WaterMyPlant.Models;
using Xamarin.Forms;

namespace WaterMyPlant.ViewModels
{
    public class MyPlantPageViewModel : BaseViewModel
    {
        public const string functionurl = "https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant?code=7p3M8YQCp1KyyaGiWDPO9TtrjQQrabuC7ZIvXiVfayYfXwh55MeCFQ==";

        //public const string functionurl = "http://localhost:7071/api/WaterThePlant?";





        private string currentMotorState;
        public string CurrentMotorState
        {
            get => currentMotorState;
            set => SetProperty(ref currentMotorState, value);
        }


        private int currentMoisturelevel;
        public int CurrentMoisturelevel
        {
            get => currentMoisturelevel;
            set => SetProperty(ref currentMoisturelevel, value);
        }




        private bool automodeenabled;
        public bool Automodeenabled
        {
            get => automodeenabled;
            set
            {
                SetProperty(ref automodeenabled, value);
                ShowManualMode = !automodeenabled;

                if (automodeenabled)
                {
                    SetConfigAutoMode(automodeenabled);
                }
            }
        }

        private bool showmanualmode;
        public bool ShowManualMode
        {
            get => showmanualmode;
            set => SetProperty(ref showmanualmode, value);
        }


        private List<ChartEntry> entry;

        public List<ChartEntry> ChartEntry
        {
            get => entry;
            set => SetProperty(ref entry, value);
        }

        public List<string> chartcolor { get; set; } = new List<string>();

        private LineChart chart;

        public LineChart lineChart
        {
            get => chart;
            set => SetProperty(ref chart, value);

        }



        internal const string Separator = "#";

        internal const string getconfig = "getconfig=true";



        public const string MOTOR_ON = "MOTOR_ON";

        public const string MOTOR_OFF = "MOTOR_OFF";



        public const string waterdetails = "waterdetails";


        public static bool AutoMode = true;

        public static string Manual = "Manual";

        public static string Auto = "Auto";


        public MyPlantPageViewModel()
        {
            Title = "Water MyPlant";
            Task.Run(async () => await GetCurrentConfigAsync(getconfig).ConfigureAwait(false));
            StartMotorCommand = new Command(async () => await PerformPostRequestAsync(false, MOTOR_ON));
            StopMotorCommand = new Command(async () => await PerformPostRequestAsync(false, MOTOR_OFF));
            ReloadCommand = new Command(async () => await GetCurrentConfigAsync(getconfig).ConfigureAwait(false));
            ModeChangedCommand = new Command(() => { ShowManualMode = !Automodeenabled; });
        }

        private async Task GetCurrentConfigAsync(string querystringandvalue)
        {
            try
            {
                lineChart = null;
                ChartEntry?.Clear();

                var client = new RestClient($"{functionurl}&{querystringandvalue}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = await client.ExecuteAsync(request);
                var data = response.Content.Split(new string[] { Separator }, StringSplitOptions.None);

                CurrentMotorState = data.ElementAt(0).Replace("\"", "");

                List<PlantWateringDeatails> wateringDeatails = null;

                if (bool.TryParse(data.ElementAt(1), out bool val))
                {
                    Automodeenabled = val;
                    showmanualmode = !val;
                }
                if (int.TryParse(data.ElementAt(2).Replace("\"", ""), out int moistval))
                {
                    CurrentMoisturelevel = moistval;
                }
                if (!string.IsNullOrEmpty(data.ElementAt(3)))
                {
                    var getjsondata = data.ElementAtOrDefault(3).Replace("\\", "").Replace("]\"", "]");

                    wateringDeatails = JsonConvert.DeserializeObject<List<PlantWateringDeatails>>(getjsondata);
                }

                if (ChartEntry == null)
                {
                    ChartEntry = new List<ChartEntry>();
                    entry = new List<ChartEntry>();
                }


                AddColorToList();
                for (int i = 0; i < wateringDeatails.Count; i++)
                {
                    entry.Add(new ChartEntry(wateringDeatails[i].MoisuteLevel)
                    {
                        Label = wateringDeatails[i].PlantingeTime.Hour.ToString(),
                        ValueLabel = $"{wateringDeatails[i].MoisuteLevel}",
                        Color = SKColor.Parse(GetCurrentColor(i)),
                    });
                }



                lineChart = new LineChart { Entries = ChartEntry };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        private void SetConfigAutoMode(bool automode)
        {
            var client = new RestClient($"{functionurl}&{nameof(automode)}={automode}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
        }

        public ICommand ModeChangedCommand { get; }

        public ICommand StartMotorCommand { get; }

        public ICommand StopMotorCommand { get; }

        public ICommand ReloadCommand { get; }

        public async Task PerformPostRequestAsync(bool automode, string motorstate)
        {
            try
            {
                int mtrval;

                if (motorstate == MOTOR_ON)
                {
                    mtrval = 1;
                }
                else
                {
                    mtrval = 0;
                }


                var client = new RestClient($"{functionurl}&{nameof(automode)}={automode}&{nameof(motorstate)}={mtrval}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = await client.ExecuteAsync(request);

                await GetCurrentConfigAsync(getconfig);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }


        public void AddColorToList()
        {
            chartcolor.AddRange(new string[] {"#FF0033",
                                            "#FF8000",
                                            "#FFE600",
                                            "#FFE400",
                                            "#FFA100",
                                            "#1AB34D",
                                            "#1A66FF",
                                            "#801AB3",
                                            "#991AFC",
                                            "#1A69FF",
                                            "#FE0243",
                                            "#823AB3" });
        }

        public string GetCurrentColor(int i)
        {
            return chartcolor.ElementAtOrDefault(i);
        }



    }
}