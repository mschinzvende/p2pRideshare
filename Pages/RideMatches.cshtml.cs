using GoogleApi.Entities.Maps.DistanceMatrix.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using p2pRideshare.DMatrixAPI;
using static p2pRideshare.DMatrixAPI.GoogleDistanceMatrixApi;

namespace p2pRideshare.Pages
{
    public class RideMatchesModel : PageModel
    {
        public async void OnGet()
        {
            await MatchingAlgorithm("Tynwald South, Zimbabwe", "Norton, Zimbabwe");

            
        }


        public async Task<GoogleDistanceMatrixApi.Response> MatchingAlgorithm(string origin, string destination)
        {
            GoogleDistanceMatrixApi api = new GoogleDistanceMatrixApi(new[] { origin }, new[] { destination });
            var response = await api.GetResponse();

            

            Response mydistance = response;

            var distance = mydistance.Rows[0];
            var newdistance = mydistance.Rows[0].Elements[0].Distance.Value;
            
            return response;
        }
    }
}
