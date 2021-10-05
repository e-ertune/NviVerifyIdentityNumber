using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NviVerifyIdentityNumber.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NviVerifyIdentityNumber.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            if (await Verify(user))
            {
                TempData["verifyResult"] = "Doğrulama başarılı.";
            }
            else
            {
                TempData["verifyResult"] = "Doğrulama başarısız.";
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<bool> Verify(User user)
        {
            HttpClient client = new HttpClient();
            String str1 = @"
            <soap12:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap12='http://www.w3.org/2003/05/soap-envelope'>
                <soap12:Body>
                    <TCKimlikNoDogrula xmlns='http://tckimlik.nvi.gov.tr/WS'>
                        <TCKimlikNo>" + user.IdentityNumber + "</TCKimlikNo>" +
                        "<Ad>" + user.FirstName + "</Ad>" +
                        "<Soyad>" + user.LastName + "</Soyad>" +
                        "<DogumYili>" + user.BirthYear + "</DogumYili>" +
                    "</TCKimlikNoDogrula>" +
                "</soap12:Body>" +
            "</soap12:Envelope>";

            HttpContent content = new StringContent(str1, Encoding.UTF8, "application/soap+xml");
            client.DefaultRequestHeaders.Add("SOAPAction", "http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula");
            var response = await client.PostAsync("https://tckimlik.nvi.gov.tr/Service/KPSPublic.asmx", content);
            var result = response.Content.ReadAsStringAsync().Result;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(result);
            return Convert.ToBoolean(xml.ChildNodes[1].FirstChild.FirstChild.FirstChild.FirstChild.InnerText);
        }
    }
}