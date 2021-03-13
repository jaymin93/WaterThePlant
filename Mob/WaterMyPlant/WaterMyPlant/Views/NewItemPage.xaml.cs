using System;
using System.Collections.Generic;
using System.ComponentModel;
using WaterMyPlant.Models;
using WaterMyPlant.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WaterMyPlant.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}