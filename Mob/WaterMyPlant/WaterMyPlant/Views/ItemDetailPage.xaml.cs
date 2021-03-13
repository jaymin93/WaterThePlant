using System.ComponentModel;
using WaterMyPlant.ViewModels;
using Xamarin.Forms;

namespace WaterMyPlant.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}