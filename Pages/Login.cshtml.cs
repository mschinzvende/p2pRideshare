using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.Models;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace p2pRideshare.Pages
{
    public class LoginModel : PageModel
    {
        public string errorMessage = "";
        public string successMessage ="";
        public void OnGet()
        {
            //Globals.logged_in_user = "";
            //Globals.user_permission = "";
            //  Globals.user_picture = "";
            // Globals.user_account_status = "";
            //Globals.logged_in_user_id = "";

            HttpContext.Session.SetString("UserID", "" );
            HttpContext.Session.SetString("UserNAME", "" );
            HttpContext.Session.SetString("UserPICTURE", "");
            HttpContext.Session.SetString("UserPERMISSION", "");
            HttpContext.Session.SetString("UserACCOUNTSTATUS", "");

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

        }

        public void OnPost()
        {
            string username = Request.Form["email"];
            string password = Request.Form["password"];
            string hashedEquivalent = computePassHash(password);


            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE userName=@userName AND password=@password";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userName", username);
                        command.Parameters.AddWithValue("@password", hashedEquivalent);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    HttpContext.Session.SetString("UserID", "" + reader.GetInt32(0));
                                    HttpContext.Session.SetString("UserNAME", "" + reader.GetString(1));
                                    HttpContext.Session.SetString("UserPICTURE", "" + reader.GetString(4));
                                    HttpContext.Session.SetString("UserPERMISSION", "" + reader.GetString(11));
                                    HttpContext.Session.SetString("UserACCOUNTSTATUS", "" + reader.GetString(10));

                                    /*Globals.logged_in_user_id = ""+ reader.GetInt32(0);
                                    Globals.logged_in_user = reader.GetString(1);
                                    Globals.user_picture = reader.GetString(4);
                                    Globals.user_permission = reader.GetString(11);
                                    Globals.user_account_status = reader.GetString(10);*/

                                }
                                


                                //navigate to the dashboard page

                                Response.Redirect("/Dashboard");

                            }

                            else if (!reader.HasRows)
                            {
                                errorMessage = "Invalid Login Details";
                                connection.Close();
                                return;
                            }

                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
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


        public void GetUpdatedAccountDetails()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE userId=@userId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId",HttpContext.Session.GetString("UserID"));


                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {


                                    HttpContext.Session.SetString("UserNAME", "");
                                    HttpContext.Session.SetString("UserPICTURE", "");
                                    HttpContext.Session.SetString("UserPERMISSION", "");
                                    HttpContext.Session.SetString("UserACCOUNTSTATUS", "");


                                    //logged_in_user = reader.GetString(1);
                                    // user_picture = reader.GetString(4);
                                    // user_permission = reader.GetString(11);
                                    // user_account_status = reader.GetString(10);

                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
