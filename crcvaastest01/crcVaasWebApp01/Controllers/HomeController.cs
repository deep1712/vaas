using crcVaasWebApp01.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace crcVaasWebApp01.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            string message = TempData["Message"] as string ?? string.Empty;
            MessageModel messageModel = new MessageModel();
            messageModel.Message = message.Replace("[","").Replace("]","").Replace("\"","");
            
            return View(messageModel);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile CsvFile, IFormFile JsonFile)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://crcvaastestdev01.azurewebsites.net/JsonValidator");
            request.Headers.Add("Cookie", "ARRAffinity=58a135dabb1761655df6bc1eed62d291be8b5144553a4eaa1630d6d114a755b0; ARRAffinitySameSite=58a135dabb1761655df6bc1eed62d291be8b5144553a4eaa1630d6d114a755b0");
            var content = new MultipartFormDataContent();

            var fileCsvStream = CsvFile.OpenReadStream();
            var fileCsvContent = new StreamContent(fileCsvStream);
            content.Add(fileCsvContent, "csvFile", CsvFile.FileName);

            var fileJsonStream = JsonFile.OpenReadStream();
            var fileJsonContent = new StreamContent(fileJsonStream);
            content.Add(fileJsonContent, "jsonFile", JsonFile.FileName);

            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            TempData["Message"] = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            return RedirectToAction("Index");
        }
    }
}