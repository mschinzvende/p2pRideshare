using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using p2pRideshare.FileUploadService;
using p2pRideshare.Models;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace p2pRideshare.Pages
{
    public class SignupModel : PageModel
    {
        
        public SignupModel(IFileUploadService fileUploadService)
        {
            FileUploadService = fileUploadService;
        }

       

        public string errorMessage = "";
        public string successMessage = "";

        public IFileUploadService FileUploadService { get; }

        public User user = new User();

        public void OnGet()
        {
        }

        public void OnPost(IFormFile idpic, IFormFile profilepic)
        {
            if (idpic != null)
            {
                user.idScan = FileUploadService.UploadFile(idpic);
            }

            if (profilepic != null)
            {
                user.profilePic = FileUploadService.UploadFile(profilepic);
            }


            user.fullName = Request.Form["fullname"];
            user.phone = Request.Form["phone"];
            user.physicalAddress = Request.Form["physicaladdress"];
            user.password = Request.Form["password"];
            user.email = Request.Form["email"];
            user.idNo = Request.Form["idno"];
            user.username = user.email;
            user.password = computePassHash(user.password);
            user.accountStatus = "Pending Review";

            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "INSERT INTO users (fullName, idNo, idScan, profilePic, phone, email, physicalAddress, userName, password, accountStatus)" +
                        "VALUES (@fullName, @idNo, @idScan, @profilePic, @phone, @email, @physicalAddress, @userName, @password, @accountStatus)";

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
                        command.Parameters.AddWithValue("@accountStatus", user.accountStatus);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                if (!SendSignUpEmail(user.email, user.fullName))
                {
                    successMessage = "There was an error sending your email confirmation. Please contact support.";
                }

                else 
                {
                    successMessage = "We have created an account for you. Please check your email for more details";
                        
                }
                
                //Response.Redirect("/?success=" + successMessage);
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
