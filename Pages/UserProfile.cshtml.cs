using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using p2pRideshare.FileUploadService;
using p2pRideshare.Models;
using System.Data.SqlClient;
using MailKit.Net.Smtp;
using System.Text;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

namespace p2pRideshare.Pages
{
    public class UserProfileModel : PageModel
    {
      
        public string errorMessage = "";
        public string successMessage = "";
        public User user = new User();

        public UserProfileModel(IFileUploadService fileUploadService) 
        {
            FileUploadService = fileUploadService;
      
        }
        public IFileUploadService FileUploadService { get; }

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
        public void OnPostChangePassword()
        {
            user.userId = Request.Form["userid"];
            user.password = Request.Form["password"];
            string hashedpass = computePassHash(user.password);

            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "UPDATE users SET password=@password WHERE userId=@userId";


                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", user.userId);
                        command.Parameters.AddWithValue("@password", hashedpass);
                        
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                if (!SendSignUpEmail(user.email, user.fullName, "Password Change"))
                {
                    successMessage = "Update Successfull but there was an error sending email confirmation. Please contact support.";
                }

                else
                {
                    successMessage = "Password Change Success";
                }
                Response.Redirect("/UserProfile?successMessage=" + successMessage + "&userId=" + user.userId);




            }



            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }


        }
        public void OnPostUpdateUser(IFormFile idpic, IFormFile profilepic)
        {
            if (idpic != null)
            {
                user.idScan = FileUploadService.UploadFile(idpic);
            }
            else
            {
                user.idScan = Request.Form["id_picture"];
            }

            if (profilepic != null)
            {
                user.profilePic = FileUploadService.UploadFile(profilepic);
            }
            else
            {
                user.profilePic = Request.Form["profile_picture"];
            }

            user.userId = Request.Form["userid"];
            user.fullName = Request.Form["fullname"];
            user.phone = Request.Form["phone"];
            user.physicalAddress = Request.Form["physicaladdress"];
            user.password = Request.Form["password"];
            user.email = Request.Form["email"];
            user.idNo = Request.Form["idno"];
            user.username = user.email;
           
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "UPDATE users SET fullName=@fullName, idNo=@idNo, idScan=@idScan, profilePic=@profilePic, phone=@phone, email=@email, physicalAddress=@physicalAddress, userName=@userName" +
                        " WHERE userId=@userId";
                       

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", user.userId);
                        command.Parameters.AddWithValue("@fullName", user.fullName);
                        command.Parameters.AddWithValue("@idNo", user.idNo);
                        command.Parameters.AddWithValue("@idScan", user.idScan);
                        command.Parameters.AddWithValue("@profilePic", user.profilePic);
                        command.Parameters.AddWithValue("@phone", user.phone);
                        command.Parameters.AddWithValue("@email", user.email);
                        command.Parameters.AddWithValue("@physicalAddress", user.physicalAddress);
                        command.Parameters.AddWithValue("@userName", user.username);
                        

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                if (!SendSignUpEmail(user.email, user.fullName))
                {
                    successMessage = "Update Successfull but there was an error sending email confirmation. Please contact support.";
                }

                else
                {
                    successMessage = "User Details Update Success";
                }
                Response.Redirect("/UserProfile?successMessage=" + successMessage+ "&userId="+user.userId);




            }



            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

        }


        public bool SendSignUpEmail(string email, string fullname, string sendingFunction="")
        {
            try
            {
                string theMessage = "";
                string theSubject = "";

                if (sendingFunction == "Password Change")
                {
                    theMessage = "Hello. Your Password has been changed. Your new password is "+ user.password;
                    theSubject = "Password Change Notification";
                }
                else
                {
                    theMessage = "Hello. This is to notify change of account information. Your username is " + email + " Please login using that username and your password";
                    theSubject = "New Account Information";
                
                }

                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("P2P Ride Share", "info@p2prideshare.co.zw"));
                mailMessage.To.Add(new MailboxAddress(fullname, email));
                mailMessage.Subject = theSubject;
                mailMessage.Body = new TextPart("plain")
                {

                    Text = theMessage
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




        public void OnGet()
        {
            

            if(string.IsNullOrEmpty(Request.Query["successMessage"]))
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



           user.userId = Request.Query["userId"];

            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string sql = "SELECT * FROM users WHERE userId=@userId";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", user.userId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                                user.userId = "" + reader.GetInt32(0);
                                user.fullName = reader.GetString(1);
                                user.idNo = reader.GetString(2);
                                user.idScan = reader.GetString(3);
                                user.profilePic = reader.GetString(4);
                                user.phone = reader.GetString(5);
                                user.email = reader.GetString(6);
                                user.physicalAddress = reader.GetString(7);
                                user.username = reader.GetString(8);
                                user.accountStatus = reader.GetString(9);
                                user.accountStatus = reader.GetString(10);
                                user.userType = reader.GetString(11);
                                user.aiVerification = reader.GetString(12);

                                

                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
    }
}
