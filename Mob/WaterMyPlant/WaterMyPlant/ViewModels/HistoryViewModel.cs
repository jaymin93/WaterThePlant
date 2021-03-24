using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WaterMyPlant.Models;
using WaterMyPlant.Views;
using Xamarin.Forms;

namespace WaterMyPlant.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        

        List<PlantWateringDeatails> items;

        public ObservableCollection<PlantWateringDeatails> Items { get; }
        public Command LoadItemsCommand { get; }
      
      

        public HistoryViewModel()
        {
            Title = "History";
            Items = new ObservableCollection<PlantWateringDeatails>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                items = await GetHistoryAsync().ConfigureAwait(false);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }



        private async Task<List<PlantWateringDeatails>> GetHistoryAsync()
        {
            try
            {
                var client = new RestClient($"{MyPlantPageViewModel.functionurl}&gethistory=true");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = await client.ExecuteAsync(request);
                var apidata = response.Content.Replace("\\", "").Replace("]\"", "]").Replace("\"[", "[");
                var data = JsonConvert.DeserializeObject<List<PlantWateringDeatails>>(apidata);

                items = data;
                return items;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
           
        }

    }
}