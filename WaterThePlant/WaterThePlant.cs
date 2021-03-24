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
        public const string MOTOR_ON = "MOTOR_ON";

        public const string MOTOR_OFF = "MOTOR_OFF";



        public const string waterdetails = "waterdetails";


        public static bool AutoMode = true;

        public static string Manual = "Manual";

        public static string Auto = "Auto";


        public const string seprator = "#";

        private const string TableName = "PlantWateringDeatails";

        public static string CurrentMotorState { get; set; }

        public static int currentMoisturelevel { get; set; }


        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        
  
        [FunctionName("WaterThePlant")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            GETRequestData getdata = new GETRequestData();

            if (int.TryParse(req.Query["moisturelevel"], out int mstlvl))
            {
                getdata.moisturelevel = mstlvl;

            }

            if (bool.TryParse(req.Query["automode"], out bool atmd))
            {
                getdata.automode = atmd;
                AutoMode = atmd;
            }

            if (bool.TryParse(req.Query["getconfig"], out bool gtcnfg))
            {
                getdata.getconfig = gtcnfg;
            }

            if (bool.TryParse(req.Query["getHistory"], out bool gthty))
            {
                getdata.getHistory = gthty;
            }

            if (bool.TryParse(req.Query["reportMoistureLevel"], out bool rptmstlvl))
            {
                getdata.reportMoistureLevel = rptmstlvl;
            }

            if (int.TryParse(req.Query["motorstate"], out int mtrstt))
            {
                getdata.motorstate = mtrstt;
            }





            if (getdata.getconfig)
            {

                List<PlantWateringDeatails> wateringdetailsfortheday = await GetPlantWateringDeatailsAsync();

                return new OkObjectResult($"{CurrentMotorState}{seprator}{AutoMode}{seprator}{currentMoisturelevel}{seprator}{JsonConvert.SerializeObject(wateringdetailsfortheday)}");

            }
            else if (getdata.getHistory)
            {
                List<PlantWateringDeatails> wateringdetailsfortheday = await GetPlantWateringDeatailsAsync();

                return new OkObjectResult($"{JsonConvert.SerializeObject(wateringdetailsfortheday)}");
            }
            else
            {



                if (getdata.moisturelevel != 0)
                {
                    currentMoisturelevel = getdata.moisturelevel;

                }


                switch (getdata.motorstate)
                {
                    case 0:
                        CurrentMotorState = MOTOR_OFF;
                        break;

                    case 1:
                        CurrentMotorState = MOTOR_ON;
                        break;
                }




                if (AutoMode)
                {
                    if (getdata.reportMoistureLevel)
                    {
                        await InsertWateringDeatilsTOAzureTable(currentMoisturelevel, $"{CurrentMotorState} due to moisture level is  {currentMoisturelevel}", log, true);

                    }
                    else
                    {
                        await InsertWateringDeatilsTOAzureTable(currentMoisturelevel, $"{CurrentMotorState} due to moisture level is {currentMoisturelevel}", log);
                    }



                    return new OkObjectResult($"{Auto}{seprator}{CurrentMotorState}{seprator}{currentMoisturelevel}");


                }
                else
                {

                    if (getdata.reportMoistureLevel)
                    {
                        await InsertWateringDeatilsTOAzureTable(currentMoisturelevel, $"{CurrentMotorState} due to moisture level is {currentMoisturelevel}", log, true);

                    }
                    else
                    {
                        await InsertWateringDeatilsTOAzureTable(currentMoisturelevel, $"{CurrentMotorState} due to moisture level is {currentMoisturelevel}", log);
                    }

                    return new OkObjectResult($"{Manual}{seprator}{CurrentMotorState}{seprator}{currentMoisturelevel}");
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
                    details = new PlantWateringDeatails($"waterdetails{DateTime.Now:dd-MM-yyyy}", $"myplant{DateTime.Now:dd-MM-yyyy-HH-mm-ss}");
                }
                else
                {
                    details = new PlantWateringDeatails(waterdetails, $"myplant{DateTime.Now:dd-MM-yyyy-HH-mm-ss}");
                }

                details.PlantingeTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);



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
                Debug.Write(exp);
                return default;
            }
        }




    }

}
