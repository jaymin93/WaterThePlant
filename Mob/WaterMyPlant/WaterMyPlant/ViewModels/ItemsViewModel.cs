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
    public class ItemsViewModel : BaseViewModel
    {
        private PlantWateringDeatails _selectedItem;

        List<PlantWateringDeatails> items;

        public ObservableCollection<PlantWateringDeatails> Items { get; }
        public Command LoadItemsCommand { get; }
      
      

        public ItemsViewModel()
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
                var client = new RestClient("https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-functions-key", "7p3M8YQCp1KyyaGiWDPO9TtrjQQrabuC7ZIvXiVfayYfXwh55MeCFQ==");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $"{{\r\n    \"getHistory\":\"true\"\r\n}}", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
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
            SelectedItem = null;
        }

        public PlantWateringDeatails SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        async void OnItemSelected(PlantWateringDeatails item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.MoisuteLevel)}={item.MoisuteLevel}");
        }
    }
}