using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using p2pRideshare.FileUploadService;
using p2pRideshare.Models;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using p2pRideshare.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Reflection.PortableExecutable;
using Org.BouncyCastle.Bcpg;

namespace p2pRideshare.Pages
{
    public class SignupModel : PageModel
    {
        
        public SignupModel(IFileUploadService fileUploadService, IWebHostEnvironment environment)
        {
            FileUploadService = fileUploadService;

            Environment = environment;
        }

       

        public string errorMessage = "";
        public string successMessage = "";

        public string matchConfidence = "0";
        public string matchError = string.Empty;

        public IFileUploadService FileUploadService { get; }

        public User user = new User();

        public IWebHostEnvironment Environment { get; }

        public void OnGet()
        {

            if (!string.IsNullOrEmpty(Request.Query["isVerified"]))
            {
                HttpContext.Session.SetString("isIDVerified", Request.Query["isVerified"]);
            }

            if (string.IsNullOrEmpty(Request.Query["isVerified"]))
            {
                HttpContext.Session.SetString("isIDVerified", "No");
            }
           

            if (string.IsNullOrEmpty(Request.Query["successMessage"]))
            {
                successMessage = "";
            }
            else
            {
                successMessage = Request.Query["successMessage"];
            }

            if (string.IsNullOrEmpty(Request.Query["errorMessage"]))
            {
                errorMessage = "";
            }
            else
            {
                errorMessage = Request.Query["errorMessage"];
            }

            if (!string.IsNullOrEmpty(Request.Query["idPic"]))
            {
                user.idScan = Request.Query["idPic"];
            }

            if (!string.IsNullOrEmpty(Request.Query["confidenceScore"]))
            {
                user.aiVerification = Request.Query["confidenceScore"];
            }

            if (!string.IsNullOrEmpty(Request.Query["profilePic"]))
            {
                user.profilePic = Request.Query["profilePic"];
            }

        }


        public async Task OnPostCompare(IFormFile idpic, IFormFile profilepic)
        {
            if (idpic != null)
            {
                user.idScan = FileUploadService.UploadFile(idpic);
            }

            if (profilepic != null)
            {
                user.profilePic = FileUploadService.UploadFile(profilepic);
            }



            string _apiUrl = "https://faceapi.mxface.ai";
            string _subscripptionKey = "JbDOOp5DtA3DSstgY7-i9NBNd3RAk2112";
            
            using (var httpClient = new HttpClient())
            {
                string filepathID = Path.Combine(Environment.ContentRootPath, @"wwwroot\mugshots", user.idScan);
                string filepathPic = Path.Combine(Environment.ContentRootPath, @"wwwroot\mugshots", user.profilePic);
                APICompareRequest request = new APICompareRequest
                {
                    encoded_image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(filepathID)),
                    encoded_image2 = Convert.ToBase64String(System.IO.File.ReadAllBytes(filepathPic)),
                    //QualityThreshold =56
                };
                string jsonRequest = JsonConvert.SerializeObject(request);
                httpClient.BaseAddress = new Uri(_apiUrl);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("subscriptionkey", _subscripptionKey);
                var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("/api/v3/face/verify", httpContent);
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MatchedFaceResponse compareFaces = JsonConvert.DeserializeObject<MatchedFaceResponse>(apiResponse);
                    foreach (var item in compareFaces.MatchedFaces)
                    {
                        matchConfidence = ""+ item.matchResult;
                    }
                    

                }
                else
                {
                   matchError = "Error {0}, {1}" + response.StatusCode + "/" + apiResponse;

                }

                if (float.Parse(matchConfidence) >= 0.5)
                {
                    Response.Redirect("/Signup?isVerified=Yes&idPic=" + user.idScan + "&profilePic=" + user.profilePic+"&confidenceScore="+matchConfidence);
                }

                else
                {
                    Response.Redirect("/Signup?isVerified=No");
                }
            }
        }
        public void OnPost()
        {



            try
            {
                if (HttpContext.Session.GetString("isIDVerified") == "No")
                {
                    errorMessage = "ID verification Failed";
                    return;
                }

                else if (HttpContext.Session.GetString("isIDVerified") == "Yes")
                {



                    

                    user.fullName = Request.Form["fullname"];
                    user.phone = Request.Form["phone"];
                    user.physicalAddress = Request.Form["physicaladdress"];
                    user.password = Request.Form["password"];
                    user.email = Request.Form["email"];
                    user.idNo = Request.Form["idno"];
                    user.username = user.email;
                    user.password = computePassHash(user.password);
                    user.userType = "General User";
                    user.subDueDate = DateTime.Now.Date.AddDays(30).ToString("dd/MM/yyyy");
                    user.idScan = Request.Form["idPicture"];
                    user.profilePic = Request.Form["profilePicture"];
                    user.aiVerification = Request.Form["confidenceScore"];

                    using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                    {
                        connection.Open();
                        string sql = "INSERT INTO users (fullName, idNo, idScan, profilePic, phone, email, physicalAddress, userName, password, accountStatus, userType, aiVerification, rating, subDueDate)" +
                            "VALUES (@fullName, @idNo, @idScan, @profilePic, @phone, @email, @physicalAddress, @userName, @password, @accountStatus, @userType, @aiVerification, @rating, @subDueDate)";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@fullName", user.fullName);
                            command.Parameters.AddWithValue("@idNo", user.idNo);
                            command.Parameters.AddWithValue("@idScan", user.idScan);
                            command.Parameters.AddWithValue("@profilePic", user.profilePic);
                            command.Parameters.AddWithValue("@phone", user.phone);
                            command.Parameters.AddWithValue("@email", user.email);
                            command.Parameters.AddWithValue("@physicalAddress", user.physicalAddress);
                            command.Parameters.AddWithValue("@userName", user.username);
                            command.Parameters.AddWithValue("@password", user.password);
                            command.Parameters.AddWithValue("@accountStatus", "Approved");
                            command.Parameters.AddWithValue("@userType", user.userType);
                            command.Parameters.AddWithValue("@aiVerification", user.aiVerification);
                            command.Parameters.AddWithValue("@rating", 0);
                            command.Parameters.AddWithValue("@subDueDate", user.subDueDate);

                            command.ExecuteNonQuery();
                        }

                        connection.Close();
                    }



                    if (!SendSignUpEmail(user.email, user.fullName))
                    {
                        successMessage = "Registration Successful but there was an error sending your email confirmation. Please contact support.";
                    }

                    else
                    {
                        successMessage = "We have created an account for you. Please Login";

                    }

                    Response.Redirect("/?successMessage=" + successMessage);

                }
            }



            catch (Exception ex)
            {
                errorMessage = ex.Message;


            }
   
        }

        public bool SendSignUpEmail(string email, string fullname)
        {
            try
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("P2P Ride Share", "info@p2prideshare.co.zw"));
                mailMessage.To.Add(new MailboxAddress(fullname, email));
                mailMessage.Subject = "New Account Confirmation";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Hello. This is to confirm your sign up. Your username is " + email + " Please login using that username and your password"
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("mail.zimbotech.co.zw", 465, true);
                    smtpClient.Authenticate("sydney@zimbotech.co.zw", "@@###SMCsida12");
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }

                //function to send an email after user has signed up
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public string computePassHash(string rawPassword)
        {
           
            
            //function to compute the password hash value for encryption

            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

            
        
    }
}
