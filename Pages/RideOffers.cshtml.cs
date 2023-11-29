using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.Models;
using System.Data.SqlClient;

namespace p2pRideshare.Pages
{
    public class RideOffersModel : PageModel
    {
       public  string successMessage = "";
        public string errorMessage = "";

        public List<MatchedOffers> MatchedOffersList = new List<MatchedOffers>();
        MatchedOffers matchedOffer = new MatchedOffers();

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

            getMatchingOffers();
        }

        public void getDriverDetails(string userID)
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
                                matchedOffer.fullName = reader.GetString(1);
                                matchedOffer.idNo = reader.GetString(2);
                                matchedOffer.idScan = reader.GetString(3);
                                matchedOffer.profilePic = reader.GetString(4);
                                matchedOffer.phone = reader.GetString(5);
                                matchedOffer.email = reader.GetString(6);
                                matchedOffer.physicalAddress = reader.GetString(7);
                                matchedOffer.rating = ""+reader.GetInt32(13);
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

        public void getMatchingOffers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string request_sql = "SELECT o.userId, o.pickupLocation, o.finalDestination, o.pickupTime, o.pickupDate, " +
                        "o.vehicleRegNo, o.vehicleMake, o.vehicleColor, o.picDriversLicense, o.picZinaraReg, matches.matchId FROM rideOffers AS o JOIN matches ON o.offerId=matches.offerId " +
                        "JOIN rideRequests ON rideRequests.requestId=matches.requestId JOIN users ON rideRequests.userId=users.userId " +
                        "WHERE rideRequests.userId=@current_user AND matches.driverStatus='Waiting' ";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(request_sql, connection))
                    {
                        command.Parameters.AddWithValue("@current_user", HttpContext.Session.GetString("UserID"));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                matchedOffer = new MatchedOffers();

                                matchedOffer.pickupLocation = reader.GetString(1);
                                matchedOffer.finalDestination = reader.GetString(2);
                                matchedOffer.pickupTime = reader.GetString(3);
                                matchedOffer.pickupDate = reader.GetString(4);
                                matchedOffer.vehicleRegNo = reader.GetString(5);
                                matchedOffer.vehicleMake = reader.GetString(6);
                                matchedOffer.vehicleColor = reader.GetString(7);
                                matchedOffer.picDriversLicense = reader.GetString(8);
                                matchedOffer.picZinaraReg = reader.GetString(9);
                                matchedOffer.matchId = "" + reader.GetInt32(10);
                                getDriverDetails("" + reader.GetInt32(0));

                                MatchedOffersList.Add(matchedOffer);


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
