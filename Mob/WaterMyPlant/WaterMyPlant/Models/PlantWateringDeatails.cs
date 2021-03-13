using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WaterMyPlant.Models
{
    public class PlantWateringDeatails:TableEntity
    {
        public int MoisuteLevel { get; set; }
        public DateTime PlantingeTime { get; set; }
        public string Message { get; set; }
    }
}