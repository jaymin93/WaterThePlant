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
using System.Linq;
using System.Linq.Expressions;

namespace WaterThePlant
{
    public static class WaterThePlant
    {
        public const string StartMotor = "StartMotor";

        public const string StopMotor = "StopMotor";

        public const string IdleMotor = "IdleMotor";

        public const string waterdetails = "waterdetails";
        public static int UpperBoundmoisturelevel { get; set; } = 80;

        public static int LowerBoundmoisturelevel { get; set; } = 20;

        public static bool AutoMode;

        public const string seprator = "##";

        private const string TableName = "PlantWateringDeatails";

        public static string CurrentMotorState { get; set; }

        public static int currentMoisturelevel { get; set; }

        [FunctionName("WaterThePlant")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            int moisturelevel;

            string content = await new StreamReader(req.Body).ReadToEndAsync();

            PostRequestData postdata = JsonConvert.DeserializeObject<PostRequestData>(content);


            if (postdata.getconfig)
            {
                List<PlantWateringDeatails> wateringdetailsfortheday = await GetPlantWateringDeatailsAsync();

                return new OkObjectResult($"{CurrentMotorState}{seprator}{AutoMode}{seprator}{currentMoisturelevel}{seprator}{JsonConvert.SerializeObject(wateringdetailsfortheday)}");
            }
            else if (postdata.getHistory)
            {
                List<PlantWateringDeatails> wateringdetailsfortheday = await GetPlantWateringDeatailsAsync();

                return new OkObjectResult($"{JsonConvert.SerializeObject(wateringdetailsfortheday)}");
            }
            else
            {
                if (!bool.TryParse(postdata.automode.ToString(), out AutoMode))
                {
                    log.LogError($"Invalid argument for Automode {postdata.automode}");
                }

                if (AutoMode)
                {
                    if (!string.IsNullOrEmpty(postdata.moisturelevel))
                    {
                        if (int.TryParse(postdata.moisturelevel, out moisturelevel))
                        {
                            currentMoisturelevel = moisturelevel;

                            if (moisturelevel <= LowerBoundmoisturelevel)
                            {
                                if (postdata.reportMoistureLevel)
                                {
                                    await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Started Motor to Plant water due to current moisture level is {moisturelevel}", log, true);

                                }
                                else
                                {
                                    await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Started Motor to Plant water due to current moisture level is {moisturelevel}", log);
                                }


                                CurrentMotorState = StartMotor;

                                return new OkObjectResult($"{StartMotor}{seprator}{AutoMode}{seprator}{currentMoisturelevel}");
                            }
                            else if (moisturelevel >= UpperBoundmoisturelevel)
                            {

                                if (postdata.reportMoistureLevel)
                                {
                                    await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Stopped Motor to Plant water due to current moisture level is {moisturelevel}", log, true);

                                }
                                else
                                {
                                    await InsertWateringDeatilsTOAzureTable(moisturelevel, $"Stopped Motor to Plant water due to current moisture level is {moisturelevel}", log);

                                }


                                CurrentMotorState = StopMotor;


                                return new OkObjectResult($"{StopMotor}{seprator}{AutoMode}{seprator}{currentMoisturelevel}");
                            }
                            else
                            {
                                CurrentMotorState = IdleMotor;

                                return new OkObjectResult($"{IdleMotor}{seprator}{AutoMode}{seprator}{currentMoisturelevel}");
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
                        if (!int.TryParse(postdata.moisturelevel, out moisturelevel))
                        {
                            log.LogError($"unable to parse moisturelevel{postdata.moisturelevel}");
                        }

                        currentMoisturelevel = moisturelevel;

                        switch (postdata.motorstate)
                        {
                            case StartMotor:
                                CurrentMotorState = StartMotor;

                                return new OkObjectResult($"{StartMotor}{seprator}{AutoMode}{seprator}{currentMoisturelevel}");

                            case StopMotor:
                                CurrentMotorState = StopMotor;

                                return new OkObjectResult($"{StopMotor}{seprator}{AutoMode}{seprator}{currentMoisturelevel}");

                            case IdleMotor:
                                CurrentMotorState = IdleMotor;

                                return new OkObjectResult($"{IdleMotor}{seprator}{AutoMode}{seprator}{currentMoisturelevel}");

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

        }


        public static async Task<bool> InsertWateringDeatilsTOAzureTable(int moisturelevel, string message, ILogger log, bool ReportMoistureLevel = false)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference(TableName);

                //can be used in situation if you are not sure table exist , in my i created table so i do not need to check this
                //await table.CreateIfNotExistsAsync();

                PlantWateringDeatails details;

                if (ReportMoistureLevel)
                {
                    details = new PlantWateringDeatails($"waterdetails{DateTime.Now:dd-MM-yyyy}", $"myplant{waterdetails}{moisturelevel}");
                }
                else
                {
                    details = new PlantWateringDeatails(waterdetails, $"myplant{waterdetails}{moisturelevel}");
                }

                string currentdatetimeinddmmyy = DateTime.Now.ToString("dd-MM-yyyy-HH");

                if (DateTime.TryParseExact(currentdatetimeinddmmyy, "dd-MM-yyyy-HH", null, System.Globalization.DateTimeStyles.None, out DateTime parseddate))
                {
                    details.PlantingeTime = parseddate;
                }

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
                return default;
            }

        }



        public static async Task<List<PlantWateringDeatails>> GetPlantWateringDeatailsAsync(bool GetHistory = false)
        {
            try
            {
                List<PlantWateringDeatails> PlantWateringDeatailsrecords = new List<PlantWateringDeatails>();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=watertheplant20210313151;AccountKey=TPcEaU2J7xk+0xy9k8/ZRgCCaSqgzOq4Uhje1KfXvcdiUALLvB0eue3GlYRsWegDmFk9NyJeH5XeSP9c3iqkrw==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable _linkTable = tableClient.GetTableReference(nameof(PlantWateringDeatails));


                TableQuery<PlantWateringDeatails> query;

                if (GetHistory)
                {
                    query = new TableQuery<PlantWateringDeatails>().Take(25).Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"waterdetails"));
                }
                else
                {
                    query = new TableQuery<PlantWateringDeatails>().Take(12).Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"waterdetails{DateTime.Now:dd-MM-yyyy}"));
                }


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




    }

}
