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
    public class AboutViewModel : BaseViewModel
    {
        public const string functionurl = "https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant";


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
            set => SetProperty(ref automodeenabled, value);
        }


        private List<ChartEntry> entry;

        public List<ChartEntry> ChartEntry
        {
            get => entry;
            set => SetProperty(ref entry, value);
        }



        private LineChart chart;

        public LineChart lineChart
        {
            get => chart;
            set => SetProperty(ref chart, value);

        }

        internal const string StartMotor = "StartMotor";

        internal const string StopMotor = "StopMotor";

        internal const string Separator = "##";

        public AboutViewModel()
        {
            Title = "WaterMyPlant";
            Task.Run(async () => await GetCurrentConfigAsync().ConfigureAwait(false));
            StartMotorCommand = new Command(async () => await PerformPostRequestAsync(false, StartMotor));
            StopMotorCommand = new Command(async () => await PerformPostRequestAsync(false, StopMotor));
        }

        private async Task GetCurrentConfigAsync()
        {
            try
            {
                lineChart = null;
                ChartEntry?.Clear();

                var client = new RestClient("https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-functions-key", "7p3M8YQCp1KyyaGiWDPO9TtrjQQrabuC7ZIvXiVfayYfXwh55MeCFQ==");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $"{{\r\n    \"getconfig\":\"true\"\r\n}}", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
                var data = response.Content.Split(new string[] { Separator }, StringSplitOptions.None);

                CurrentMotorState = data.ElementAt(0).Replace("\"", "");

                List<PlantWateringDeatails> wateringDeatails = null;

                if (bool.TryParse(data.ElementAt(1), out bool val))
                {
                    Automodeenabled = val;
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

                foreach (var item in wateringDeatails)
                {
                    entry.Add(new ChartEntry(item.MoisuteLevel)
                    {
                        Label = "9 AM",
                        ValueLabel = $"{item.MoisuteLevel}",
                        Color = SKColor.Parse("#FF0033"),
                    });
                }


                lineChart = new LineChart { Entries = ChartEntry };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public ICommand OpenWebCommand { get; }

        public ICommand StartMotorCommand { get; }

        public ICommand StopMotorCommand { get; }

        public async Task PerformPostRequestAsync(bool automodevalue, string motorstate)
        {
            try
            {
                var client = new RestClient("https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-functions-key", "7p3M8YQCp1KyyaGiWDPO9TtrjQQrabuC7ZIvXiVfayYfXwh55MeCFQ==");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $"{{\r\n    \"moisturelevel\":\"1\",\r\n    \"automode\":\"{automodevalue}\",\r\n    \"motorstate\":\"{motorstate}\"\r\n}}", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);

                await GetCurrentConfigAsync();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

    }
}