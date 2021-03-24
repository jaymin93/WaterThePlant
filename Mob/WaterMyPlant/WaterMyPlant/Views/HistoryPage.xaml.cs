using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaterMyPlant.Models;
using WaterMyPlant.ViewModels;
using WaterMyPlant.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WaterMyPlant.Views
{
    public partial class HistoryPage : ContentPage
    {
        HistoryViewModel _viewModel;

        public HistoryPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new HistoryViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}