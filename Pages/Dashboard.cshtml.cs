using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.DMatrixAPI;
using p2pRideshare.FileUploadService;
using p2pRideshare.Models;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Web.Mvc;
using Org.BouncyCastle.Security;

namespace p2pRideshare.Pages
{
    public class DashboardModel : PageModel
    {


        public DashboardModel(IFileUploadService fileUploadService)
        {
            FileUploadService = fileUploadService;
        }

        public IFileUploadService FileUploadService { get; }

        Offers offer = new Offers();
        Requests request = new Requests();

        public string errorMessage = "";
        public string successMessage = "";
        public void OnGet()
        {
            //Globals.GetUpdatedAccountDetails();
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

        public void OnPostGetARide()
        {
            request.userId=  HttpContext.Session.GetString("UserID");
            request.pickupLocation = Request.Form["requestpickuplocation"];
            request.dropoffLocation = Request.Form["reqdestinationLocation"];
            request.laguageDescription = Request.Form["lugageDescription"];
            request.extraPassengers = Request.Form["extraPassengers"];
            request.offerPrice = Request.Form["offerprice"];
            request.pickupTime = Request.Form["pickupTime"];
            request.pickupDate = Request.Form["date_day"] + "/"+ Request.Form["date_month"] +"/"+Request.Form["date_year"];


            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "INSERT INTO rideRequests (userId, pickupLocation, dropoffLocation, laguageDescription, extraPassengers, " +
                        "offerPrice, pickupTime, pickupDate) VALUES (@userId, @pickupLocation, @dropoffLocation, @laguageDescription, @extraPassengers," +
                        " @offerPrice, @pickupTime, @pickupDate)";



                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", request.userId);
                        command.Parameters.AddWithValue("@pickupLocation", request.pickupLocation);
                        command.Parameters.AddWithValue("@dropoffLocation", request.dropoffLocation);
                        command.Parameters.AddWithValue("@laguageDescription", request.laguageDescription);
                        command.Parameters.AddWithValue("@extraPassengers", request.extraPassengers);
                        command.Parameters.AddWithValue("@offerPrice", request.offerPrice);
                        command.Parameters.AddWithValue("@pickupTime", request.pickupTime);
                        command.Parameters.AddWithValue("@pickupDate", request.pickupDate);

                        command.ExecuteNonQuery();


                    }
                    connection.Close();

                    successMessage = "Request Submitted Successfully. Driver matches will show below. Kindly wait for a match";
                    Response.Redirect("/RideOffers?successMessage=" + successMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        public void OnPostOfferRide(IFormFile licensePic, IFormFile zinaraPic, IFormFile vehiclePic)
        {
            if (licensePic != null)
            {
                offer.picDriversLicense = FileUploadService.UploadFile(licensePic);
            }
            else
            {
                offer.picDriversLicense = "";
            }

            if (zinaraPic != null)
            {
                offer.picZinaraReg = FileUploadService.UploadFile(zinaraPic);
            }
            else
            {
                offer.picZinaraReg = "";
            }

            if (vehiclePic != null)
            {
                offer.picCar = FileUploadService.UploadFile(vehiclePic);
            }
            else
            {
                offer.picCar = "";
            }


            offer.userId = HttpContext.Session.GetString("UserID");
            offer.pickupLocation = Request.Form["pickupLocation"];
            offer.pickupDestination = Request.Form["destinationLocation"];
            offer.pickupTime = Request.Form["pickupTime"];
            offer.pickupDate = Request.Form["date_day"] + "/" + Request.Form["date_month"] + "/" + Request.Form["date_year"];
            offer.vehicleRegNo = Request.Form["vehicleRegNo"];
            offer.vehicleMake = Request.Form["vehicleMake"];
            offer.vehicleColor = Request.Form["vehicleColor"];
            offer.pickupThreshold = Request.Form["pickupThreshold"];
            offer.destinationThreshold = Request.Form["dropoffThreshold"];

            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    connection.Open();
                    string sql = "INSERT INTO rideOffers (userId, pickupLocation, finalDestination, pickupTime, pickupDate, " +
                        "vehicleRegNo, vehicleMake, vehicleColor, pickupThreshold, destinationThreshold, picDriversLicense, picZinaraReg, picCar)" +
                        "VALUES (@userId, @pickupLocation, @pickupDestination, @pickupTime, @pickupDate, @vehicleRegNo, @vehicleMake, @vehicleColor, " +
                        " @pickupThreshold, @destinationThreshold, @picDriversLicense, @picZinaraReg, @picCar)";


                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", offer.userId);
                        command.Parameters.AddWithValue("@pickupLocation", offer.pickupLocation);
                        command.Parameters.AddWithValue("@pickupDestination", offer.pickupDestination);
                        command.Parameters.AddWithValue("@pickupTime", offer.pickupTime);
                        command.Parameters.AddWithValue("@pickupDate", offer.pickupDate);
                        command.Parameters.AddWithValue("@vehicleRegNo", offer.vehicleRegNo);
                        command.Parameters.AddWithValue("@vehicleMake", offer.vehicleMake);
                        command.Parameters.AddWithValue("@vehicleColor", offer.vehicleColor);
                        command.Parameters.AddWithValue("@pickupThreshold", offer.pickupThreshold);
                        command.Parameters.AddWithValue("@destinationThreshold", offer.destinationThreshold);
                        command.Parameters.AddWithValue("@picDriversLicense", offer.picDriversLicense);
                        command.Parameters.AddWithValue("@picZinaraReg", offer.picZinaraReg);
                        command.Parameters.AddWithValue("@picCar", offer.picCar);


                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    successMessage = "Offer Submitted Successfully. Passenger matches will show below. Kindly wait for a match";
                    Response.Redirect("/RideRequests?successMessage=" + successMessage);
                }

                }

            catch(Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

    }
}
