using System.Configuration;
using System.Net;
using Predica.FimCommunication;

namespace Predica.FimExplorer.UI.WPF
{
    public class FimClientFactory
    {
        public static IFimClient CreateClient()
        {
            var url = ConfigurationManager.AppSettings["fimAddress"];
            var username = ConfigurationManager.AppSettings["fimUser"];
            var password = ConfigurationManager.AppSettings["fimPassword"];

            return new FimClient(url, new NetworkCredential(username, password));
        }
    }
}