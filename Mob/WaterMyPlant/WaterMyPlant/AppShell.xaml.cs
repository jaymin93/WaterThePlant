using System;
using System.Collections.Generic;
using WaterMyPlant.ViewModels;
using WaterMyPlant.Views;
using Xamarin.Forms;

namespace WaterMyPlant
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
