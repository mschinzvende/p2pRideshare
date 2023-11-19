using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace p2pRideshare
{
    public class Globals
    {
        public static string connection_string = "Data Source=DESKTOP-B34DOTS\\SQLEXPRESS;Initial Catalog=p2prideshare;Integrated Security=True";
        // public static string logged_in_user = "";
        // public static string user_permission = "";
        //public static string user_picture = "";
        //public static string user_account_status = "";
        //public static string logged_in_user_id = "";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISession session;
        public Globals(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            session = _httpContextAccessor.HttpContext.Session;

            
        }



        
    }
}
