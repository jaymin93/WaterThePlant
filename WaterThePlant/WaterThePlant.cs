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
using System.Diagnostics;

namespace WaterThePlant
{
    public static class WaterThePlant
    {
        public const string StartMotor = "StartMotor";

        public const string StopMotor = "StopMotor";

        public const string IdleMotor = "idleMotor";

        public const string waterdetails = "waterdetails";
        public static int UpperBoundmoisturelevel { get; set; } = 80;

        public static int LowerBoundmoisturelevel { get; set; } = 20;

        public static bool AutoMode { get; set; }

        public const string seprator = "##"; 

        [FunctionName("WaterThePlant")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            int moisturelevel;

            string content = await new StreamReader(req.Body).ReadToEndAsync();

            PostRequestData postdata = JsonConvert.DeserializeObject<PostRequestData>(content);


            AutoMode = postdata.automode;

            if (AutoMode)
            {
                if (!string.IsNullOrEmpty(postdata.moisturelevel))
                {
                    if (int.TryParse(postdata.moisturelevel, out moisturelevel))
                    {
                        if (moisturelevel <= LowerBoundmoisturelevel)
                        {
                            await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Started Motor to Plant water due to current moisture level is {moisturelevel}", log);

                            return new OkObjectResult($"{StartMotor}{seprator}{AutoMode}");
                        }
                        else if (moisturelevel >= UpperBoundmoisturelevel)
                        {
                            await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Stopped Motor to Plant water due to current moisture level is {moisturelevel}", log);

                            return new OkObjectResult($"{StopMotor}{seprator}{AutoMode}");
                        }
                        else
                        {
                            return new OkObjectResult($"{IdleMotor}{seprator}{AutoMode}");
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
            else
            {
                if (!string.IsNullOrEmpty(postdata.motorstate))
                {
                    switch (postdata.motorstate)
                    {
                        case StartMotor:
                            return new OkObjectResult($"{StartMotor}{seprator}{AutoMode}");

                        case StopMotor:
                            return new OkObjectResult($"{StopMotor}{seprator}{AutoMode}");

                        case IdleMotor:
                            return new OkObjectResult($"{IdleMotor}{seprator}{AutoMode}");

                        default:
                            return new BadRequestObjectResult("Error : Invalid moisture level argument.please provide correct value for the moisture leval");
                           
                    }

                }
                else
                {
                    return new BadRequestObjectResult("Error : Invalid moisture level argument.please provide correct value for the moisture leval");
                }
            }
            
        }


        public static async Task<bool> InsertWateringDeatilsTOAzureTable(int moisturelevel, string message,ILogger log)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("PlantWateringDeatails");

                //can be used in situation if you are not sure table exist , in my i created table so i do not need to check this
                //await table.CreateIfNotExistsAsync();

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
                log.LogError(ex.ToString());
                return  default;
            }
            
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


    public class PostRequestData
    {
        public string moisturelevel { get; set; }

        public bool automode { get; set; }

        public string motorstate { get; set; }
    }

}
