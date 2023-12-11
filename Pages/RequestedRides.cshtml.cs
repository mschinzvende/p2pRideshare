using MailKit.Net.Imap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Asn1.Ocsp;
using p2pRideshare.Models;
using System.Data.SqlClient;

namespace p2pRideshare.Pages
{
    public class RequestedRidesModel : PageModel
    {
        public string errorMessage = string.Empty;  
        public string successMessage = string.Empty;

        

        public List<RequestInfoStatus> requestsForUserList = new List<RequestInfoStatus>();


        public void OnPost()
        {
            string laguageDescription = Request.Form["laguageDescription"];
            string extraPassengers = Request.Form["extraPassengers"];
            string offerPrice = Request.Form["offerPrice"];
            string pickupTime = Request.Form["pickupTime"];
            string pickupDate = Request.Form["pickupDate"];
            string requestID = Request.Form["requestId"];


            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "UPDATE rideRequests SET laguageDescription = @laguageDescription, extraPassengers = @extraPassengers, offerPrice = @offerPrice, " +
                        "pickupTime = @pickupTime, pickupDate = @pickupDate WHERE requestId = @requestId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@laguageDescription", laguageDescription);
                        command.Parameters.AddWithValue("@extraPassengers", extraPassengers);
                        command.Parameters.AddWithValue("@offerPrice", offerPrice);
                        command.Parameters.AddWithValue("@pickupTime", pickupTime);
                        command.Parameters.AddWithValue("@pickupDate", pickupDate);
                        command.Parameters.AddWithValue("@requestId", requestID);

                        command.ExecuteNonQuery();

                    }

                    connection.Close();

                    successMessage = "Saved Successfully";

                    Response.Redirect("/RequestedRides?successMessage="+successMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        public void OnGet()
        {
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

            string requestId = Request.Query["requestId"];

            string action = Request.Query["action"];


            if (action == "Cancel")
            {
                CancelRequest(requestId);
            }

            if(action == "Delete")
            {
                DeleteRequest(requestId);
            }

            GetSubmittedRequests();
        }


        public void DeleteRequest(string requestId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "DELETE FROM rideRequests WHERE requestId=@requestId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestId);
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                    successMessage = "Request Deleted Success";

                    Response.Redirect("/RequestedRides?successMessage=" + successMessage);

                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        public void CancelRequest(string requestId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "UPDATE matches SET passengerStatus = @passengerStatus, driverStatus=@passengerStatus WHERE requestId=@requestId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@passengerStatus", "Cancelled");
                        command.Parameters.AddWithValue("@requestId", requestId);
                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    successMessage = "Request Cancelled Success";

                    Response.Redirect("/RequestedRides?successMessage=" + successMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }



        public string GetRequestStatus(string requestId)
        {
            string status = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "SELECT driverStatus FROM matches WHERE requestId=@requestId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {


                                status = reader.GetString(0);
                            }
                        }
                    }
                }
                return status;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return string.Empty;
            }
        }

        public void GetSubmittedRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "SELECT * FROM rideRequests WHERE userId=@current_user";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@current_user", HttpContext.Session.GetString("UserID"));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RequestInfoStatus aRequest = new RequestInfoStatus();

                                aRequest.pickupLocation = reader.GetString(2);
                                aRequest.dropoffLocation = reader.GetString(3);
                                aRequest.laguageDescription = reader.GetString(4);
                                aRequest.extraPassengers = reader.GetString(5);
                                aRequest.offerPrice = reader.GetString(6);
                                aRequest.pickupTime = reader.GetString(7);
                                aRequest.pickupDate = reader.GetString(8);
                                aRequest.requestStatus = GetRequestStatus(""+reader.GetInt32(0));
                                aRequest.requestId = "" + reader.GetInt32(0);
                                

                                requestsForUserList.Add(aRequest);
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
