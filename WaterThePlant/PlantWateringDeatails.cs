using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace WaterThePlant
{
    public class PlantWateringDeatails : TableEntity
    {
        public PlantWateringDeatails()
        {

        }
        public PlantWateringDeatails(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }
        public DateTime PlantingeTime { get; set; }
        public int MoisuteLevel { get; set; }
        public string Message { get; set; }

    }

}
