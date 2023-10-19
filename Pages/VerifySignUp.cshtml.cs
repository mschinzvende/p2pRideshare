using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.Models;
using System.Data;
using System.Data.SqlClient;

namespace p2pRideshare.Pages
{
    public class VerifySignUpModel : PageModel
    {
        public List<User> userslist = new List<User>();
        public string errorMessage = "";
        public string successMessage = "";

        public void OnPostSearchUser()
        {
            string natId = Request.Form["idSearch"];

            Response.Redirect("/VerifySignUp?userId=" + natId);


        }


        private void PerformUserAction(string action, string userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "UPDATE users SET accountStatus=@action" +
                        " WHERE userId=@userId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@userId", userId);


                        command.ExecuteNonQuery();

                    }

                    connection.Close();
                }

                successMessage = "User Status set to " + action;

                Response.Redirect("/VerifySignUp?successMessage="+successMessage);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        public void OnGet()
        {
            successMessage = Request.Query["successMessage"];

            if (successMessage == null)
            {
                successMessage = "";
            }
            string sql = "";
            if (!string.IsNullOrEmpty(Request.Query["userId"]))
            {
                string natID = Request.Query["userId"];
                 sql = "SELECT * FROM users WHERE idNo='"+natID+"'";

            }

            else
            {
                sql = "SELECT * FROM users";
            }

            if (!string.IsNullOrEmpty(Request.Query["verifAction"]))
            {
                PerformUserAction(Request.Query["verifAction"], Request.Query["userId"]);
            }
            
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User();
                                user.userId = reader.GetInt32(0);
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

                                userslist.Add(user);

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


        //new fxn here



    }
}
