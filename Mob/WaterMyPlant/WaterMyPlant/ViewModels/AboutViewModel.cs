using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
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




        public AboutViewModel()
        {
            Title = "WaterMyPlant";
            Task.Run(async () => await GetCurrentConfigAsync().ConfigureAwait(false));
            StartMotorCommand = new Command(async () => await PerformPostRequestAsync(false, "StartMotor"));
            StopMotorCommand = new Command(async () => await PerformPostRequestAsync(false, "StopMotor"));
        }

        private async Task GetCurrentConfigAsync()
        {
            try
            {
                var client = new RestClient("https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-functions-key", "7p3M8YQCp1KyyaGiWDPO9TtrjQQrabuC7ZIvXiVfayYfXwh55MeCFQ==");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $"{{\r\n    \"moisturelevel\":\"1\",\r\n    \"automode\":\"false\",\r\n    \"motorstate\":\"StartMotor\"\r\n}}", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);
                var data = response.Content.Split(new string[] { "##" }, StringSplitOptions.None);

                CurrentMotorState = data.ElementAt(0);
                if (bool.TryParse(data.ElementAt(1), out bool val))
                {
                    Automodeenabled = val;

                }

            }
            catch (Exception ex)
            {

                var x = ex;
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
                
            }
            catch (Exception ex)
            {

                var x = ex;
            }

        }

    }


    public class PostRequestData
    {
        public string moisturelevel { get; set; }

        public bool automode { get; set; }

        public string motorstate { get; set; }
    }
}