using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WaterThePlantModel
{
    public class PlantWateringDeatails : TableEntity
    {

        public PlantWateringDeatails(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }
        public DateTime DateTime { get; set; }
        public int MoisuteLevel { get; set; }
        public string Message { get; set; }

    }
}
