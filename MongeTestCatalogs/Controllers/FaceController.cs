using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace MongeTestCatalogs.Controllers
{
    public class FaceController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;

        public FaceController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Capture")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capture(string base64String)
        {

            //if (!string.IsNullOrEmpty(base64String))
            //{
            //    var imageParts = base64String.Split(',').ToList<string>();
            //    byte[] imageBytes = Convert.FromBase64String(imageParts[1]);
            //    DateTime nm = DateTime.Now;
            //    string date = nm.ToString("yyyymmddMMss");

            //    string webRootPath = _hostingEnvironment.WebRootPath;
            //    string path = Path.Combine(webRootPath, "CapturedPhotos");
            //    path = Path.Combine(path, date + "CamCapture.jpg");

            //    var response = await MakeAnalysisRequest(imageBytes);

            //    System.IO.File.WriteAllBytes(path, imageBytes);
            //    return new JsonResult("'data': "+ response);
            //}
            //else
            //{
            //    return new JsonResult("'data': false");
            //}
            return new JsonResult("'data': false");
        }

        public static async Task<string> MakeAnalysisRequest(byte[] imageBytes)
        {
            const string subscriptionKey = "6b0875bdb8214351ba461904de13a39d";
            const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

            HttpClient client = new HttpClient();

            // Request headers.  
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".  
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.  
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.  
            byte[] byteData = imageBytes;

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".  
                // The other content types you can use are "application/json" and "multipart/form-data".  
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.  
                response = await client.PostAsync(uri, content);

                // Get the JSON response.  
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.  

                return JsonPrettyPrint(contentString);

            }
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}