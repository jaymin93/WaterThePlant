using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using RestSharp;
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
        List<PlantWateringDeatails> items;


        public const string PlantWateringDeatails = "PlantWateringDeatails";




        public MockDataStore()
        {
            var datafromapi = Task.Run(async () => await GetHistoryAsync().ConfigureAwait(false));
        }

        //public async void GetDataFromAzureTable()
        //{
        //    items = await GetPlantWateringDeatailsAsync().ConfigureAwait(false);
        //}

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

        public async Task<List<PlantWateringDeatails>> GetPlantWateringDeatailsAsync()
        {
            try
            {
                List<PlantWateringDeatails> PlantWateringDeatailsrecords = new List<PlantWateringDeatails>();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable _linkTable = tableClient.GetTableReference(PlantWateringDeatails);


                TableQuery<PlantWateringDeatails> query = new TableQuery<PlantWateringDeatails>().Take(10).Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "waterdetails"));


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

                        PlantWateringDeatailsrecords.Add(_summary);
                    }
                } while (token != null);

                return PlantWateringDeatailsrecords;
            }
            catch (Exception exp)
            {
                return null;
            }
        }


        private async Task<List<PlantWateringDeatails>> GetHistoryAsync()
        {
            try
            {
                var client = new RestClient("https://watertheplant20210313151300.azurewebsites.net/api/WaterThePlant");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("x-functions-key", "7p3M8YQCp1KyyaGiWDPO9TtrjQQrabuC7ZIvXiVfayYfXwh55MeCFQ==");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", $"{{\r\n    \"getHistory\":\"true\"\r\n}}", ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request).ConfigureAwait(false);
                var apidata = response.Content.Replace("\\", "").Replace("]\"", "]").Replace("\"[", "[");
                var data = JsonConvert.DeserializeObject<List<PlantWateringDeatails>>(apidata);

                items = data;
                return items;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

    }
}
