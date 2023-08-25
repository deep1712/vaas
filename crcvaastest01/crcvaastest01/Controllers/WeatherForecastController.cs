using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json.Linq;

namespace crcvaastest01.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JsonValidatorController : ControllerBase
    {
        private readonly ILogger<JsonValidatorController> _logger;

        public JsonValidatorController(ILogger<JsonValidatorController> logger)
        {
            _logger = logger;
        }

        private List<int> GetLineNumbers(string text, string key, string val, StringComparison comparison = StringComparison.CurrentCulture)
        {
            List<int> lineNums = new List<int>();
            int lineNum = 0;
            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNum++;
                    if (line.Contains(key, comparison) && line.Contains(val, comparison))
                        lineNums.Add(lineNum);
                }
            }
            return lineNums;
        }

        private bool IsJArray(string str)
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

        private bool IsJObject(string str)
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

        private void CheckValues(string json, Dictionary<string, string> dictionary, ref HashSet<String> errors)
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
                        CheckValues(vl, dictionary, ref errors);
                    }
                    else
                    {
                        if (dictionary.ContainsKey(kv.Key))
                        {
                            string expectedValue = dictionary[kv.Key].Split(',')[1];
                            List<int> lineNums = this.GetLineNumbers(json, kv.Key, kv.Value.ToString());
                            string lineNumString = String.Join(',', lineNums);

                            string errorMessage = $"Errors at line numbers: {lineNumString} ; Error Message: {dictionary[kv.Key].Split(',')[2]} ; Invalid Key value pair: \'{kv.Key}: {kv.Value}\'";
                            if (!Regex.IsMatch(vl, expectedValue, RegexOptions.IgnoreCase))
                            {
                                errors.Add(errorMessage.Trim());
                            }
                        }
                    }
                }
            }
        }

        [HttpPost(Name = "Post")]
        public string Post(IFormFile csvFile, IFormFile jsonFile)
        {
            HashSet<string> errors = new HashSet<string>();

            using (var csvStream = new StreamReader(csvFile.OpenReadStream()))
            {
                // Read the contents of the CSV file
                var csvData = csvStream.ReadToEnd();
                using (var jsonStream = new StreamReader(jsonFile.OpenReadStream()))
                {
                    List<string> listStr = new List<string>(csvData.Split("\n", StringSplitOptions.RemoveEmptyEntries));

                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    if (listStr != null)
                    {
                        foreach (string vl in listStr)
                        {
                            string key = vl.Split(',')[0];
                            dic[key] = vl;
                        }
                    }

                    string json = jsonStream.ReadToEnd();
                    CheckValues(json, dic, ref errors);
                }
            }
            return string.Join(",,,", errors.ToList());
        }
    }
}