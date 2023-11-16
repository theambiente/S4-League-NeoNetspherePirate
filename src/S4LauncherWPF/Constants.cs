using S4LauncherWPF;
using System.Net;

namespace S4LauncherWPF
{
    internal class Constants
    {
        public static bool KRClient = false;

        public static MainWindow LoginWindow;
        public static IPEndPoint ConnectEndPoint = new IPEndPoint(IPAddress.Parse("78.31.67.237"), 28001);
    }
}
