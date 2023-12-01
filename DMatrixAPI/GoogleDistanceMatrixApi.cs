using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace p2pRideshare.DMatrixAPI
{
    public class GoogleDistanceMatrixApi
    {
        public class Response
        {
            public string Status { get; set; }

            [JsonProperty(PropertyName = "origin_addresses")]
            public string[] OriginAddresses { get; set; }

            [JsonProperty(PropertyName = "destination_addresses")]
            public string[] DestinationAddresses { get; set; }

            public Row[] Rows { get; set; }

            public class Data
            {
                public int Value { get; set; }
                public string Text { get; set; }
            }

            public class Element
            {
                public string Status { get; set; }
                public Data Duration { get; set; }
                public Data Distance { get; set; }
            }

            public class Row
            {
                public Element[] Elements { get; set; }
            }
        }

        private string Key { get; set; }
        private string Url { get; set; }

        private string[] OriginAddresses { get; set; }
        private string[] DestinationAddresses { get; set; }

        public GoogleDistanceMatrixApi(string[] originAddresses, string[] destinationAddresses)
        {
            OriginAddresses = originAddresses;
            DestinationAddresses = destinationAddresses;

            //var appSettings = ConfigurationManager.AppSettings;

            if (string.IsNullOrEmpty("https://maps.googleapis.com/maps/api/distancematrix/json"))
            {
                throw new Exception("GoogleDistanceMatrixApiUrl is not set in AppSettings.");
            }
            Url = "https://maps.googleapis.com/maps/api/distancematrix/json";

            if (string.IsNullOrEmpty("AIzaSyCZ8jy-y4qWUJZL9m2ydWYlE2R5P5ZTiRE"))
            {
                throw new Exception("GoogleDistanceMatrixApiKey is not set in AppSettings.");
            }
            Key = "AIzaSyCZ8jy-y4qWUJZL9m2ydWYlE2R5P5ZTiRE";
        }

        public async Task<Response> GetResponse()
        {
            using (var client = new HttpClient())
            {
                var uri = new Uri(GetRequestUrl());

                HttpResponseMessage response = await client.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("GoogleDistanceMatrixApi failed with status code: " + response.StatusCode);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Response>(content);
                }
            }
        }

        private string GetRequestUrl()
        {
            OriginAddresses = OriginAddresses.Select(HttpUtility.UrlEncode).ToArray();
            var origins = string.Join("|", OriginAddresses);
            DestinationAddresses = DestinationAddresses.Select(HttpUtility.UrlEncode).ToArray();
            var destinations = string.Join("|", DestinationAddresses);
            return $"{Url}?origins={origins}&destinations={destinations}&key={Key}";
        }
    }
}
