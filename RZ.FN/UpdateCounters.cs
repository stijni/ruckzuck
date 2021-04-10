﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Xml;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RZ.Server;

namespace RZ.ServerFN
{
    public static class UpdateCounters
    {
        static MemoryCache memoryCache = MemoryCache.Default;
        public static Dictionary<string, string> Settings { get; set; }

        [FunctionName("UpdateCounters")]
        public static void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = false)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"UpdateCounters started at: {DateTime.Now}");

            try
            {
                ProcessDLQueue(log);
                ProcessFailureQueue(log);
                ProcessSuccessQueue(log);
            }
            catch { }

            ProcessSWQueue(log);
            log.ToString();
        }

        private static void ProcessDLQueue(ILogger log)
        {
            string sURL = Settings["dlqURL"];
            string sasToken = Settings["dlqSAS"].TrimStart('?');

            List<string> DLQueue = new List<string>();
            Dictionary<string, string> IDQueue = new Dictionary<string, string>();
            int iMessageCount = 33;

            //Get bulk of 32 Messages
            while (iMessageCount >= 32)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}?numofmessages=32&{sasToken}";
                    var oRes = oClient.GetStringAsync(url);
                    oRes.Wait();
                    string sXML = oRes.Result.ToString();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sXML);
                    iMessageCount = xmlDoc.SelectNodes("QueueMessagesList/QueueMessage").Count;

                    foreach (XmlNode xNode in xmlDoc.SelectNodes("QueueMessagesList/QueueMessage"))
                    {
                        string sMessageID = xNode["MessageId"].InnerText;
                        string ShortName = xNode["MessageText"].InnerText;
                        string sPopReceipt = xNode["PopReceipt"].InnerText;
                        DLQueue.Add(ShortName);
                        IDQueue.Add(sMessageID, sPopReceipt);
                    }
                }
            }

            var counts = DLQueue.GroupBy(x => x)
                 .ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in counts)
            {
                log.LogInformation($"Downloads: {item.Key} + {item.Value} : {DateTime.Now}");
                Inc(item.Key, item.Value, "known", "Downloads");
            }

            foreach (var sID in IDQueue)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}/{sID.Key}?{sasToken}&popreceipt={HttpUtility.UrlEncode(sID.Value)}";
                    var oRes = oClient.DeleteAsync(url);
                    oRes.Wait();
                    oRes.Result.ToString();
                }
            }
        }

        private static void ProcessFailureQueue(ILogger log)
        {
            //string sURL = Environment.GetEnvironmentVariable("faSAS").Split('?')[0];
            //string sasToken = Environment.GetEnvironmentVariable("faSAS").Substring(sURL.Length + 1);

            string sURL = Settings["faqURL"];
            string sasToken = Settings["faqSAS"].TrimStart('?');

            List<string> DLQueue = new List<string>();
            Dictionary<string, string> IDQueue = new Dictionary<string, string>();
            int iMessageCount = 33;

            //Get bulk of 32 Messages
            while (iMessageCount >= 32)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}?numofmessages=32&{sasToken}";
                    var oRes = oClient.GetStringAsync(url);
                    oRes.Wait();
                    string sXML = oRes.Result.ToString();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sXML);
                    iMessageCount = xmlDoc.SelectNodes("QueueMessagesList/QueueMessage").Count;

                    foreach (XmlNode xNode in xmlDoc.SelectNodes("QueueMessagesList/QueueMessage"))
                    {
                        string sMessageID = xNode["MessageId"].InnerText;
                        string ShortName = xNode["MessageText"].InnerText;
                        string sPopReceipt = xNode["PopReceipt"].InnerText;
                        DLQueue.Add(ShortName);
                        IDQueue.Add(sMessageID, sPopReceipt);
                    }
                }
            }

            var counts = DLQueue.GroupBy(x => x)
                 .ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in counts)
            {
                log.LogInformation($"Failures: {item.Key} + {item.Value} : {DateTime.Now}");
                Inc(item.Key, item.Value, "known", "Failures");
            }

            foreach (var sID in IDQueue)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}/{sID.Key}?{sasToken}&popreceipt={HttpUtility.UrlEncode(sID.Value)}";
                    var oRes = oClient.DeleteAsync(url);
                    oRes.Wait();
                    oRes.Result.ToString();
                }
            }
        }

        private static void ProcessSuccessQueue(ILogger log)
        {
            //string sURL = Environment.GetEnvironmentVariable("suSAS").Split('?')[0];
            //string sasToken = Environment.GetEnvironmentVariable("suSAS").Substring(sURL.Length + 1);

            string sURL = Settings["suqURL"];
            string sasToken = Settings["suqSAS"].TrimStart('?');

            List<string> DLQueue = new List<string>();
            Dictionary<string, string> IDQueue = new Dictionary<string, string>();
            int iMessageCount = 33;

            //Get bulk of 32 Messages
            while (iMessageCount >= 32)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}?numofmessages=32&{sasToken}";
                    var oRes = oClient.GetStringAsync(url);
                    oRes.Wait();
                    string sXML = oRes.Result.ToString();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sXML);
                    iMessageCount = xmlDoc.SelectNodes("QueueMessagesList/QueueMessage").Count;

                    foreach (XmlNode xNode in xmlDoc.SelectNodes("QueueMessagesList/QueueMessage"))
                    {
                        string sMessageID = xNode["MessageId"].InnerText;
                        string ShortName = xNode["MessageText"].InnerText;
                        string sPopReceipt = xNode["PopReceipt"].InnerText;
                        DLQueue.Add(ShortName);
                        IDQueue.Add(sMessageID, sPopReceipt);
                    }
                }
            }

            var counts = DLQueue.GroupBy(x => x)
                 .ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in counts)
            {
                log.LogInformation($"Success: {item.Key} + {item.Value} : {DateTime.Now}");
                Inc(item.Key, item.Value, "known", "Success");
            }

            foreach (var sID in IDQueue)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}/{sID.Key}?{sasToken}&popreceipt={HttpUtility.UrlEncode(sID.Value)}";
                    var oRes = oClient.DeleteAsync(url);
                    oRes.Wait();
                    oRes.Result.ToString();
                }
            }
        }

        private static void ProcessSWQueue(ILogger log)
        {
            //string sURL = Environment.GetEnvironmentVariable("swSAS").Split('?')[0];
            //string sasToken = Environment.GetEnvironmentVariable("swSAS").Substring(sURL.Length + 1);

            string sURL = Settings["swqURL"];
            string sasToken = Settings["swqSAS"].TrimStart('?');

            List<string> DLQueue = new List<string>();
            Dictionary<string, string> IDQueue = new Dictionary<string, string>();
            int iMessageCount = 33;
            int icount = 0;
            //Get bulk of 32 Messages
            while (iMessageCount >= 32 && icount <= 20)
            {
                using (HttpClient oClient = new HttpClient())
                {
                    string url = $"{sURL}?numofmessages=32&{sasToken}";
                    var oRes = oClient.GetStringAsync(url);
                    oRes.Wait();
                    string sXML = oRes.Result.ToString();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sXML);
                    iMessageCount = xmlDoc.SelectNodes("QueueMessagesList/QueueMessage").Count;
                    icount++;
                    foreach (XmlNode xNode in xmlDoc.SelectNodes("QueueMessagesList/QueueMessage"))
                    {
                        try
                        {
                            string sMessageID = xNode["MessageId"].InnerText;
                            string JSON = xNode["MessageText"].InnerText;
                            string sPopReceipt = xNode["PopReceipt"].InnerText;
                            //DLQueue.Add(ShortName);

                            JObject jMsg = JObject.Parse(JSON);
                            string manufacturer = clean(jMsg["Manufacturer"].Value<string>()).Trim().ToLower();
                            string productname = clean(jMsg["ProductName"].Value<string>()).Trim().ToLower(); ;
                            string productversion = clean(jMsg["ProductVersion"].Value<string>()).Trim().ToLower();
                            string shortname = clean(jMsg["ShortName"].Value<string>()).Trim().ToLower();

                            string sID = Hash.CalculateMD5HashString((manufacturer + productname + productversion).Trim());

                            string result = memoryCache[sID] as string;

                            if (string.IsNullOrEmpty(result))
                            {
                                if (string.IsNullOrEmpty(shortname))
                                {
                                    InsertEntityAsync(Settings["lookURL"] + "?" + Settings["lookSAS"], "lookup", sID, JSON);
                                    shortname = "JSON";
                                }
                                else
                                {
                                    UpdateEntityAsync(Settings["lookURL"] + "?" + Settings["lookSAS"], "lookup", sID, JSON);
                                }

                                memoryCache.Set(sID, shortname, DateTimeOffset.Now.AddHours(4));
                            }
                            else
                            {
                                if (result != shortname)
                                {
                                    //only update if shortname is available
                                    if (!string.IsNullOrEmpty(shortname))
                                    {
                                        UpdateEntityAsync(Settings["lookURL"] + "?" + Settings["lookSAS"], "lookup", sID, JSON);
                                    }
                                }
                            }

                            IDQueue.Add(sMessageID, sPopReceipt);
                        }
                        catch (Exception ex)
                        {
                            //remove the corrupt message from queue...
                            try
                            {
                                string sMessageID2 = xNode["MessageId"].InnerText;
                                string sPopReceipt2 = xNode["PopReceipt"].InnerText;
                                IDQueue.Add(sMessageID2, sPopReceipt2);
                            }
                            catch { }
                        }
                    }
                }
            }

            foreach (var sID in IDQueue)
            {
                try
                {
                    using (HttpClient oClient = new HttpClient())
                    {
                        string url = $"{sURL}/{sID.Key}?{sasToken}&popreceipt={HttpUtility.UrlEncode(sID.Value)}";
                        var oRes = oClient.DeleteAsync(url);
                        oRes.Wait();
                        oRes.Result.ToString();
                    }
                }
                catch(Exception ex) 
                {
                    ex.Message.ToString();
                }
            }
        }

        public static string clean(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return "";

            return (string.Join("", filename.Split(Path.GetInvalidFileNameChars()))).Trim().TrimEnd('.');
        }

        private static void Inc(string ShortName, int count, string PartKey = "known", string AttributeName = "Downloads")
        {
            //string sURL = Environment.GetEnvironmentVariable("catSAS").Split('?')[0];
            //string sasToken = Environment.GetEnvironmentVariable("catSAS").Substring(sURL.Length + 1);

            string sURL = Settings["catURL"];
            string sasToken = Settings["catSAS"].TrimStart('?');

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(sURL + "()?$filter=PartitionKey eq '" + PartKey + "' and shortname eq '" + WebUtility.UrlEncode(ShortName.ToLower()) + "' and IsLatest eq true&$select=PartitionKey,RowKey," + AttributeName + "&" + sasToken);

                request.Method = "GET";
                request.Headers.Add("x-ms-version", "2017-04-17");
                request.Headers.Add("x-ms-date", DateTime.Now.ToUniversalTime().ToString("R"));
                request.Accept = "application/json";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var content = string.Empty;
                string ETag = "";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }

                var jres = JObject.Parse(content);

                JArray jResult = jres["value"] as JArray;
                ETag = jResult[0]["odata.etag"].ToString();
                string PartitionKey = jResult[0]["PartitionKey"].ToString();
                string RowKey = jResult[0]["RowKey"].ToString();
                int iCount = jResult[0][AttributeName].Value<int>();
                iCount = iCount + count;
                JObject jUpd = new JObject();
                jUpd.Add(AttributeName, iCount);

                MergeEntityAsync(sURL + "?" + sasToken, PartitionKey, RowKey, jUpd.ToString(), ETag);
            }
            catch { }

        }

        private static void MergeEntityAsync(string url, string PartitionKey, string RowKey, string JSON, string ETag = "*")
        {
            try
            {
                string sasToken = url.Substring(url.IndexOf("?") + 1);
                string sURL = url.Substring(0, url.IndexOf("?"));

                url = sURL + "(PartitionKey='" + PartitionKey + "',RowKey='" + RowKey + "')?" + sasToken;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var jObj = JObject.Parse(JSON);
                using (HttpClient oClient = new HttpClient())
                {
                    oClient.DefaultRequestHeaders.Accept.Clear();
                    oClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    oClient.DefaultRequestHeaders.Add("If-Match", ETag);
                    HttpContent oCont = new StringContent(jObj.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
                    oCont.Headers.Add("x-ms-version", "2017-04-17");
                    oCont.Headers.Add("x-ms-date", DateTime.Now.ToUniversalTime().ToString("R"));
                    var oRes = oClient.PatchAsync(url, oCont);
                    oRes.Wait();
                }

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        private static async void InsertEntityAsync(string url, string PartitionKey, string RowKey, string JSON)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var jObj = JObject.Parse(JSON);
                jObj.Add("PartitionKey", PartitionKey);
                jObj.Add("RowKey", RowKey);
                using (HttpClient oClient = new HttpClient())
                {
                    oClient.DefaultRequestHeaders.Accept.Clear();
                    oClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent oCont = new StringContent(jObj.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
                    oCont.Headers.Add("x-ms-version", "2017-04-17");
                    oCont.Headers.Add("Prefer", "return-no-content");
                    oCont.Headers.Add("x-ms-date", DateTime.Now.ToUniversalTime().ToString("R"));
                    var oRes = await oClient.PostAsync(url, oCont);
                }

            }
            catch(Exception ex) 
            {
                ex.Message.ToString();
            }
        }

        private static async void UpdateEntityAsync(string url, string PartitionKey, string RowKey, string JSON)
        {
            try
            {
                string sasToken = url.Substring(url.IndexOf("?") + 1);
                string sURL = url.Substring(0, url.IndexOf("?"));

                url = sURL + "(PartitionKey='" + PartitionKey + "',RowKey='" + RowKey + "')?" + sasToken;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var jObj = JObject.Parse(JSON);

                using (HttpClient oClient = new HttpClient())
                {
                    oClient.DefaultRequestHeaders.Accept.Clear();
                    oClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //oClient.DefaultRequestHeaders.Add("If-Match", "*");
                    HttpContent oCont = new StringContent(jObj.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
                    oCont.Headers.Add("x-ms-version", "2017-04-17");
                    //oCont.Headers.Add("Prefer", "return-no-content");
                    //oCont.Headers.Add("If-Match", "*");
                    oCont.Headers.Add("x-ms-date", DateTime.Now.ToUniversalTime().ToString("R"));
                    var oRes = await oClient.PutAsync(url, oCont);
                }

            }
            catch(Exception ex)
            {
                ex.Message.ToString();
            }
        }
    }
}
