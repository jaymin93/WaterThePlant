using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcharts;
using SkiaSharp;
using Xamarin.Forms;

namespace WaterMyPlant.Views
{
    public partial class AboutPage : ContentPage
    {
        private readonly List<ChartEntry> _entries = new List<ChartEntry>()
        {
            new ChartEntry(70)
            {
                Label = "9 AM",
                ValueLabel = "70",
                Color = SKColor.Parse("#FF0033"),
            },
            new ChartEntry(65)
            {
                Label = "10 AM",
                ValueLabel = "65",
                Color = SKColor.Parse("#FF8000"),
            },
            new Microcharts.ChartEntry(61)
            {
                Label = "11 AM",
                ValueLabel = "61",
                Color = SKColor.Parse("#FFE600"),
            },
            new Microcharts.ChartEntry(54)
            {
                Label = "12 PM",
                ValueLabel = "54",
                Color = SKColor.Parse("#1AB34D"),
            },
            new Microcharts.ChartEntry(49)
            {
                Label = "1 PM",
                ValueLabel = "49",
                Color = SKColor.Parse("#1A66FF"),
            },
            new Microcharts.ChartEntry(42)
            {
                Label = "2 PM",
                ValueLabel = "42",
                Color = SKColor.Parse("#801AB3"),
            },
        };

        public AboutPage()
        {
            InitializeComponent();
            MyLineChart.Chart = new LineChart { Entries = _entries };
        }
    }
}