using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.Models;
using System.Data.SqlClient;

namespace p2pRideshare.Pages
{
    public class SubmittedOffersModel : PageModel
    {
        public string errorMessage = string.Empty;
        public string successMessage = string.Empty;



        public List<OfferStatus> offersForUserList = new List<OfferStatus>();


        public void OnPost()
        {
            string offerId = Request.Form["offerId"];
            string pickupTime = Request.Form["pickupTime"];
            string pickupDate = Request.Form["pickupDate"];
            



            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "UPDATE rideOffers SET pickupTime = @pickupTime, pickupDate = @pickupDate WHERE offerId = @offerId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        
                        command.Parameters.AddWithValue("@pickupTime", pickupTime);
                        command.Parameters.AddWithValue("@pickupDate", pickupDate);
                        command.Parameters.AddWithValue("@offerId", offerId);

                        command.ExecuteNonQuery();

                    }

                    connection.Close();

                    successMessage = "Saved Successfully";

                    Response.Redirect("/SubmittedOffers?successMessage=" + successMessage);
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

            string offerId = Request.Query["offerId"];

            string action = Request.Query["action"];


            if (action == "Cancel")
            {
              CancelOffer(offerId);
            }

            if (action == "Delete")
            {
                DeleteOffer(offerId);
            }

            GetSubmittedOffers();
        }


        public void DeleteOffer(string offerId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "DELETE FROM rideOffers WHERE offerId=@offerId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@offerId", offerId);
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                    successMessage = "Request Deleted Success";

                    Response.Redirect("/SubmittedOffers?successMessage=" + successMessage);

                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        public void CancelOffer(string offerId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "UPDATE matches SET passengerStatus = @driverStatus, driverStatus=@driverStatus WHERE offerId=@offerId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@driverStatus", "Cancelled");
                        command.Parameters.AddWithValue("@offerId", offerId);
                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    successMessage = "Offer Cancelled Success";

                    Response.Redirect("/SubmittedOffers?successMessage=" + successMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }



        public string GetOfferStatus(string offerId)
        {
            string status = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "SELECT passengerStatus FROM matches WHERE offerId=@offerId";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@offerId", offerId);
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

        public void GetSubmittedOffers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "SELECT * FROM rideOffers WHERE userId=@current_user";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@current_user", HttpContext.Session.GetString("UserID"));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OfferStatus aOffer = new OfferStatus();

                                aOffer.pickupLocation = reader.GetString(2);
                                aOffer.pickupDestination = reader.GetString(3);
                                aOffer.pickupTime = reader.GetString(4);
                                aOffer.pickupDate = reader.GetString(5);
                                aOffer.vehicleRegNo = reader.GetString(6);
                                aOffer.vehicleMake = reader.GetString(7);
                                aOffer.vehicleColor = reader.GetString(8);
                                

                                aOffer.offerStatus = GetOfferStatus("" + reader.GetInt32(0));
                                aOffer.offerId = "" + reader.GetInt32(0);


                                offersForUserList.Add(aOffer);
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
