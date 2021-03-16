using RestSharp;
using System;
using System.Collections.Generic;
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
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
            StartMotorCommand = new Command(async () => await PerformPostRequestAsync(false, "StartMotor"));
            StopMotorCommand =  new  Command(async () => await PerformPostRequestAsync(false, "StopMotor"));
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
                IRestResponse response = await   client.ExecuteAsync(request);
                Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {

                var x = ex;
            }
         
        }

    }
}