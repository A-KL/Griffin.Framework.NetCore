using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Http;

using Windows.Networking.Connectivity;

namespace WebServerDemo
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            var listener = new HttpListener();
            listener.MessageReceived = OnMessage;
            listener.Start(IPAddress.Parse(GetLocalIp()), 8080);

        }

        private static void OnMessage(ITcpChannel channel, object message)
        {
            var request = (IHttpRequest)message;
            var response = request.CreateResponse();

            if (request.Uri.AbsolutePath == "/favicon.ico")
            {
                response.StatusCode = 404;
                channel.Send(response);
                return;
            }

            var msg = Encoding.UTF8.GetBytes("Hello world");
            response.Body = new MemoryStream(msg);
            response.ContentType = "text/plain";
            channel.Send(response);

            if (request.HttpVersion == "HTTP/1.0")
                channel.Close();
        }

        private static string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address
            return hostname?.CanonicalName;
        }
    }
}
