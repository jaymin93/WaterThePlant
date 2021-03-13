using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WaterMyPlant.Models;
using Xamarin.Forms;

namespace WaterMyPlant.ViewModels
{
    [QueryProperty(nameof(MoisuteLevel), nameof(MoisuteLevel))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private int moisuteLevel;
        private DateTime plantingeTime;
        private string message;
        

        public DateTime PlantingeTimeText
        {
            get => plantingeTime;
            set => SetProperty(ref plantingeTime, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public int MoisuteLevel
        {
            get
            {
                return moisuteLevel;
            }
            set
            {
                moisuteLevel = value;
                LoadItemId(value);
            }
        }

        public async void LoadItemId(int itemId)
        {
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                moisuteLevel = item.MoisuteLevel;
                plantingeTime = item.PlantingeTime;
                message = item.Message;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
    }
}
