using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.FileUploadService;
using p2pRideshare.Models;
using System.Data.SqlClient;

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
            user.retypePassowrd = Request.Form["retypepassword"];
            user.email = Request.Form["email"];
            user.idNo = Request.Form["idno"];
            user.username = user.email;
            user.password = "Temporary Password";

            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "INSERT INTO users (fullName, idNo, idScan, profilePic, phone, email, physicalAddress, userName, password)" +
                        "VALUES (@fullName, @idNo, @idScan, @profilePic, @phone, @email, @physicalAddress, @userName, @password)";

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

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                successMessage = "Your account has been created successfully. Please login";
                //Response.Redirect("/?success=" + successMessage);
            }



            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            



        }
    }
}
