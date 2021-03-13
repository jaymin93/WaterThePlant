using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WaterMyPlant.Models;

namespace WaterMyPlant.Services
{
    public class MockDataStore : IDataStore<PlantWateringDeatails>
    {
        readonly List<PlantWateringDeatails> items;

        public MockDataStore()
        {
            items = GetPlantWateringDeatailsAsync().Result;
        }

        //public async Task<bool> AddItemAsync(PlantWateringDeatails item)
        //{
        //    items.Add(item);

        //    return await Task.FromResult(true);
        //}

        //public async Task<bool> UpdateItemAsync(PlantWateringDeatails item)
        //{
        //    var oldItem = items.Where((PlantWateringDeatails arg) => arg.MoisuteLevel == item.MoisuteLevel).FirstOrDefault();
        //    items.Remove(oldItem);
        //    items.Add(item);

        //    return await Task.FromResult(true);
        //}

        //public async Task<bool> DeleteItemAsync(string id)
        //{
        //    var oldItem = items.Where((PlantWateringDeatails arg) => arg.MoisuteLevel == id).FirstOrDefault();
        //    items.Remove(oldItem);

        //    return await Task.FromResult(true);
        //}

        public async Task<PlantWateringDeatails> GetItemAsync(int id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.MoisuteLevel == id));
        }

        public async Task<IEnumerable<PlantWateringDeatails>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }

        public static async Task<List<PlantWateringDeatails>> GetPlantWateringDeatailsAsync()
        {
            try
            {
                //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                //    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //    CloudTable table = tableClient.GetTableReference("PlantWateringDeatails");

                //    var gettabledataoperation = TableOperation.Retrieve("waterdetails", "myplantwaterdetails");

                //    TableResult result = await table.ExecuteAsync(gettabledataoperation).ConfigureAwait(false);


                //    List<PlantWateringDeatails> detailslist = new List<PlantWateringDeatails>();

                //    TableContinuationToken token = null;
                //    do
                //    {


                //        foreach (var entity in result.Results)
                //        {
                //            LinkSummary _summary = new LinkSummary
                //            {
                //                Raw_URL = entity.Raw_URL,
                //                Short_Code = entity.Short_Code
                //            };

                //            _records.Add(_summary);
                //        }
                //    } while (token != null);

                //}
                //catch (Exception ex)
                //{
                //    var x = ex;
                //    throw;
                //}




                List<PlantWateringDeatails> _records = new List<PlantWateringDeatails>();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable _linkTable = tableClient.GetTableReference("PlantWateringDeatails");



                // Construct the query operation for all customer entities where PartitionKey="Smith".
                TableQuery<PlantWateringDeatails> query = new TableQuery<PlantWateringDeatails>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "waterdetails"));

                // Print the fields for each customer.
                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<PlantWateringDeatails> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    foreach (var entity in resultSegment.Results)
                    {
                        PlantWateringDeatails _summary = new PlantWateringDeatails
                        {
                            Message = entity.Message,
                            MoisuteLevel = entity.MoisuteLevel,
                            PlantingeTime = entity.PlantingeTime

                        };

                        _records.Add(_summary);
                    }
                } while (token != null);


                return _records;



            }
            catch (Exception exp)
            {
                return null;
            }
        }
    }

}
