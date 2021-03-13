using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace WaterThePlant
{
    public static class WaterThePlant
    {
        public const string StartMotor = "StartMotor";

        public const string StopMotor = "StopMotor";

        public const string idleMotor = "idleMotor";

        public const string waterdetails = "waterdetails";
        public static int UpperBoundmoisturelevel { get; set; } = 80;

        public static int LowerBoundmoisturelevel { get; set; } = 20;

        [FunctionName("WaterThePlant")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            int moisturelevel;

            if (!string.IsNullOrEmpty(req.Query["moisturelevel"]))
            {
                if (int.TryParse(req.Query["moisturelevel"], out moisturelevel))
                {
                    if (moisturelevel <= LowerBoundmoisturelevel)
                    {
                        await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Started Motor to Plant water due to current moisture level is {moisturelevel}");

                       // var data = await GetPlantWateringDeatailsAsync();

                        return new OkObjectResult(StartMotor);
                    }
                    else if (moisturelevel >= UpperBoundmoisturelevel)
                    {
                        await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Stopped Motor to Plant water due to current moisture level is {moisturelevel}");

                        return new OkObjectResult(StopMotor);
                    }
                    else
                    {
                        return new OkObjectResult(idleMotor);
                    }
                }
                else
                {
                    return new BadRequestObjectResult("Error : Invalid moisture level argument.please provide correct value for the moisture leval");
                }
            }
            else
            {
                return new BadRequestObjectResult("Error : Invalid moisture level argument.please provide correct value for the moisture leval");
            }
        }


        public static async Task<bool> InsertWateringDeatilsTOAzureTable(int moisturelevel, string message)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("PlantWateringDeatails");

                await table.CreateIfNotExistsAsync();

                PlantWateringDeatails details = new PlantWateringDeatails(waterdetails, $"myplant{waterdetails}{moisturelevel}");

                details.PlantingeTime = DateTime.Now;
                details.Message = message;
                details.MoisuteLevel = moisturelevel;

                TableOperation insertOperation = TableOperation.Insert(details);

                var insertoperationresult = await table.ExecuteAsync(insertOperation);

                var sts = insertoperationresult.HttpStatusCode;

                return true;
            }
            catch (Exception ex)
            {
                var x = ex;
                throw;
            }
            
        }




        public static async Task<List<PlantWateringDeatails>> GetPlantWateringDeatailsAsync()
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("PlantWateringDeatails");

                var gettabledataoperation = TableOperation.Retrieve("waterdetails", "myplantwaterdetails");

                TableResult result = await table.ExecuteAsync(gettabledataoperation).ConfigureAwait(false);

                return JsonConvert.DeserializeObject<List<PlantWateringDeatails>>(result.Result.ToString());

            }
            catch (Exception ex)
            {
                var x = ex;
                throw;
            }

            //var insertoperationresult = await table.ExecuteAsync(insertOperation);

            //var sts = insertoperationresult.HttpStatusCode;

            //return true;

        }

    }


    public class PlantWateringDeatails : TableEntity
    {

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
