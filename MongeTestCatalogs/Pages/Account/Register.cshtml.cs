using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongeTestCatalogs.Data;
using MongeTestCatalogs.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MongeTestCatalogs.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;

        private IHostingEnvironment _hostingEnvironment;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            IEmailSender emailSender,
            IHostingEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "faceID")]
            public string FaceID { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, faceID = Input.FaceID };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(Input.Email, callbackUrl);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    
                    return LocalRedirect(Url.GetLocalUrl(returnUrl));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        //public async Task<IActionResult> OnGetMakeFaceAPIRequest(string username, string password)

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnGetRegFace(string username, string password)
        {
            try {
                var client = new HttpClient();
                var queryString = HttpUtility.ParseQueryString(string.Empty);

                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "6b0875bdb8214351ba461904de13a39d");

                var uri = "https://southcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/grupo-monge/persons";

                HttpResponseMessage response;

                // Request body
                byte[] byteData = Encoding.UTF8.GetBytes("{ 'name':  '" + username + "' , 'userData':  '" + password + "' } ");

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PutAsync(uri, content);
                }

                return new JsonResult(response);
            }
            catch (Exception e)
            {
                return new JsonResult(e);
            }
           
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCapture(string base64String)
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
