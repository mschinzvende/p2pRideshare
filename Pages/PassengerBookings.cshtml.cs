using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.Models;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace p2pRideshare.Pages
{
    public class PassengerBookingsModel : PageModel
    {
        string successMessage = "";
        string errorMessage = "";

        public List<MatchedRequest> MatchedRequestsList = new List<MatchedRequest>();
        MatchedRequest matchedRequest = new MatchedRequest();

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

            string matchId = Request.Query["matchId"];

            string action = Request.Query["action"];

            if (action == "Accept")
            {
                acceptMatch(matchId);
            }

            if (action == "Reject")
            {
                rejectMatch(matchId);

                Response.Redirect("/RideRequests");
            }

           

            getMatchingRequests();
        }

        public void acceptMatch(string matchId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string sql = "UPDATE matches SET passengerStatus=@passengerStatus WHERE matchId=@matchId";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@passengerStatus", "Accepted");
                        command.Parameters.AddWithValue("@matchId", matchId);

                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }

            }

            catch (Exception ex)
            {

            }
        }

        public void rejectMatch(string matchId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string sql = "UPDATE matches SET passengerStatus=@passengerStatus WHERE matchId=@matchId";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@passengerStatus", "Rejected");
                        command.Parameters.AddWithValue("@matchId", matchId);

                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }

            }

            catch (Exception ex)
            {

            }
        }

        public void getPassengerDetails(string userID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string user_sql = "SELECT * FROM users WHERE userId=@userId";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(user_sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                matchedRequest.fullName = reader.GetString(1);
                                matchedRequest.idNo = reader.GetString(2);
                                matchedRequest.idScan = reader.GetString(3);
                                matchedRequest.profilePic = reader.GetString(4);
                                matchedRequest.phone = reader.GetString(5);
                                matchedRequest.email = reader.GetString(6);
                                matchedRequest.physicalAddress = reader.GetString(7);
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

        public void getMatchingRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "SELECT r.userId, r.pickupLocation, r.dropoffLocation, r.laguageDescription, r.extraPassengers, " +
                        "r.offerPrice, r.pickupTime, r.pickupDate, matches.passengerStatus, matches.driverStatus FROM rideRequests AS r JOIN matches ON r.requestId=matches.requestId " +
                        "JOIN rideOffers ON rideOffers.offerId=matches.offerId JOIN users ON rideOffers.userId=users.userId " +
                        "WHERE rideOffers.userId=@current_user AND matches.passengerStatus='Accepted' ";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@current_user", HttpContext.Session.GetString("UserID"));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                matchedRequest = new MatchedRequest();

                                matchedRequest.pickupLocation = reader.GetString(1);
                                matchedRequest.dropoffLocation = reader.GetString(2);
                                matchedRequest.laguageDescription = reader.GetString(3);
                                matchedRequest.extraPassengers = reader.GetString(4);
                                matchedRequest.offerPrice = reader.GetString(5);
                                matchedRequest.pickupTime = reader.GetString(6);
                                matchedRequest.pickupDate = reader.GetString(7);
                                matchedRequest.passengerStatus = reader.GetString(8);
                                matchedRequest.driverStatus = reader.GetString(9);
                                getPassengerDetails("" + reader.GetInt32(0));

                                MatchedRequestsList.Add(matchedRequest);


                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }
    }
}
