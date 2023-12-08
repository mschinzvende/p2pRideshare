using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;
using p2pRideshare.DMatrixAPI;
using p2pRideshare.Models;
using System.Data.SqlClient;

namespace p2pRideshare.Services
{
    public class PollForMatches : BackgroundService
    {
        //private readonly TimeSpan _period = TimeSpan.FromSeconds(89);
        private readonly ILogger<PollForMatches> _logger;

        private static List<Offers> offersListForMatching = new List<Offers>();
        private static List<Requests> requestsListForMatching = new List<Requests>();

       

        public PollForMatches(ILogger<PollForMatches> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested)
            {
               await getRequests();
               await getOffers();
                // await FindMatches();
                await FindMatches2();

            }
                //Run matching algorithm


            
        }


        public async Task FindMatches2()
        {
            try
            {

                if (offersListForMatching.Count > 0)
                {

                    int PickupLocationDistance = await GetDistance(offersListForMatching[offersListForMatching.Count - 1].pickupLocation, requestsListForMatching[requestsListForMatching.Count - 1].pickupLocation);
                    int DropOffLocationDistance = await GetDistance(offersListForMatching[offersListForMatching.Count - 1].pickupDestination, requestsListForMatching[requestsListForMatching.Count - 1].dropoffLocation);



                    if (PickupLocationDistance <= Int32.Parse(offersListForMatching[offersListForMatching.Count - 1].pickupThreshold) * 1000)
                    {
                        if (DropOffLocationDistance <= Int32.Parse(offersListForMatching[offersListForMatching.Count - 1].destinationThreshold) * 1000)
                        {
                            saveMatch(Int32.Parse(requestsListForMatching[requestsListForMatching.Count - 1].requestId), Int32.Parse(offersListForMatching[offersListForMatching.Count - 1].offerId));
                            _logger.LogInformation($"{offersListForMatching[offersListForMatching.Count - 1].pickupLocation}");
                        }
                    }


                }
                
            }
            catch (Exception ex)
            {

            }



        }

        public  async Task FindMatches()
        {
            try
            {

                foreach (var offeritem in offersListForMatching)
                {
                    foreach (var requestitem in requestsListForMatching)
                    {

                        int PickupLocationDistance = await GetDistance(offeritem.pickupLocation, requestitem.pickupLocation);
                        int DropOffLocationDistance = await GetDistance(offeritem.pickupDestination, requestitem.dropoffLocation);



                        if (PickupLocationDistance <= Int32.Parse(offeritem.pickupThreshold) * 1000)
                        {
                            if (DropOffLocationDistance <= Int32.Parse(offeritem.destinationThreshold) * 1000)
                            {
                                saveMatch(Int32.Parse(requestitem.requestId), Int32.Parse(offeritem.offerId));
                                _logger.LogInformation($"{offeritem.pickupLocation}");
                            }
                        }


                    }
                }
          }
           catch (Exception ex)
           {

            }
           


        }

        public void saveMatch(int requestid, int offerid)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string sql = "INSERT INTO matches (requestId, offerId, driverStatus, passengerStatus, matchIdentifier, passengerRated, driverRated)" +
                        " VALUES (@requestId, @offerId, @driverstatus, @passengerstatus, @matchIdentifier, @passengerRated, @driverRated)";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestid);
                        command.Parameters.AddWithValue("@offerId", offerid);
                        command.Parameters.AddWithValue("@driverstatus", "Waiting");
                        command.Parameters.AddWithValue("@passengerstatus", "Waiting");
                        command.Parameters.AddWithValue("@passengerRated", "No");
                        command.Parameters.AddWithValue("@driverRated", "No");

                        int identityTotal = requestid + offerid;
                        command.Parameters.AddWithValue("@matchIdentifier", "" + identityTotal);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
           }
            catch (Exception ex)
            {

            }
        }
        public async Task getRequests()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string sql = "SELECT requestId, pickupLocation, dropoffLocation" +
                        " FROM rideRequests";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Requests matchingRequest = new Requests();
                                matchingRequest.requestId = "" + reader.GetInt32(0);
                                matchingRequest.pickupLocation = reader.GetString(1);
                                matchingRequest.dropoffLocation = reader.GetString(2);

                                requestsListForMatching.Add(matchingRequest);
                               
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

        public async Task getOffers()
        {
            try
           {
                using (SqlConnection connection = new SqlConnection(Globals.connection_string))
                {
                    string sql = "SELECT offerId, pickupLocation, finalDestination, pickupThreshold, destinationThreshold" +
                        " FROM rideOffers";
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                            using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Offers matchOffer = new Offers();

                                matchOffer.offerId = ""+ reader.GetInt32(0);
                                matchOffer.pickupLocation = reader.GetString(1);
                                matchOffer.pickupDestination = reader.GetString(2);
                                matchOffer.pickupThreshold = reader.GetString(3);
                                matchOffer.destinationThreshold = reader.GetString(4);

                                offersListForMatching.Add(matchOffer);
                                
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

        public async Task<int> GetDistance(string origin, string destination)
        {
            GoogleDistanceMatrixApi api = new GoogleDistanceMatrixApi(new[] { origin }, new[] { destination });
            var response = await api.GetResponse();
            

            int newdistance = response.Rows[0].Elements[0].Distance.Value;

            return newdistance;
        }
    }
}
