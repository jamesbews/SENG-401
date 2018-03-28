using System.Threading;
using System.Web;

namespace LinkShortener_Group14.Models
{
    /// <summary>
    /// This class contains functions and variables relevant to many different places in the web server.
    /// </summary>
    public static class Globals
    {


        /// <summary>
        /// Gets Ip
        /// </summary>
        /// <param name="user">The user name</param>
        public static string getIp()
        {
            return HttpContext.Current.Session["ip"].ToString();
        }


    }
}