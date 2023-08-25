using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System;

namespace CRCVAAS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        public bool IsJArray(string str)
        {
            bool flag = true;
            try
            {
                JArray.Parse(str);
            }
            catch
            {
                flag = false;
            }

            return flag;
        }

        public  bool IsJObject(string str)
        {
            bool flag = true;
            try
            {
                JObject.Parse(str);
            }
            catch
            {
                flag = false;
            }

            return flag;
        }

        public void CheckValues(string json, Dictionary<string, string> dictionary, ref List<String> errors)
        {
            
            if (IsJArray(json))
            {
                JArray jArr = JArray.Parse(json);
                foreach (var jObj in jArr)
                {
                    CheckValues(jObj.ToString(), dictionary, ref errors);
                }
            }
            else
            {
                JObject jObj = JObject.Parse(json);
                foreach (var kv in jObj)
                {
                    string vl = kv.Value.ToString();
                    if (IsJArray(vl) || IsJObject(vl))
                    {
                        CheckValues(vl, dictionary,ref errors);
                    }
                    else
                    {
                        if (dictionary.ContainsKey(kv.Key))
                        {
                            string expectedValue = dictionary[kv.Key].Split(',')[1];
                            string errorMessage = dictionary[kv.Key].Split(',')[2];
                            if (!Regex.IsMatch(vl, expectedValue, RegexOptions.IgnoreCase))
                            {
                                errors.Add(errorMessage);
                            }
                        }
                    }
                }
            }
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public bool Post(IFormFile csvFile, IFormFile jsonFile)
        {
            List<string> errors = new List<string>();

            using (var csvStream = new StreamReader(csvFile.OpenReadStream()))
            {
                // Read the contents of the CSV file
                var csvData = csvStream.ReadToEnd();
                using (var jsonStream = new StreamReader(jsonFile.OpenReadStream()))
                {
                    List<string> listStr = new List<string>(csvData.Split("\n", StringSplitOptions.RemoveEmptyEntries));

                    Dictionary<string, string> dic = new Dictionary<string, string>();

                    foreach (string vl in listStr)
                    {
                        string key = vl.Split(',')[0];
                        dic[key] = vl;
                    }

                    string json = jsonStream.ReadToEnd();
                    CheckValues(json, dic, ref errors);
                }
            }
            return (errors.Count == 0);
        }
    }
}