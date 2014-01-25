using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Org.Mentalis.Proxy.Http;

namespace Org.Mentalis.Proxy.HttpNms
{
    public class HttpNmsClient:HttpClient
    {
        static Regex _mask= new Regex(@"\/getDev\.asp\?map=(.*\.xml)\?(\d+),(\d+)");
        public HttpNmsClient(Socket ClientSocket, DestroyDelegate Destroyer) : base(ClientSocket, Destroyer)
        {
        }

        protected override string  RebuildQuery()
        {
            if (Host == "10.60.23.40" && RequestedPath.StartsWith("/getDev.asp?map="))
            {
                var match = _mask.Match(RequestedPath);
                if (match.Success)
                {
                    var map = match.Groups[1].Value;
                    var x = Int32.Parse(match.Groups[2].Value) + 163;
                    var y = Int32.Parse(match.Groups[3].Value) + 57;
                    RequestedPath = string.Format("/getDev.asp?map={0}?{1},{2}", map, x, y);
                }
            }
            var rc = base.RebuildQuery();
            return rc;
        }
    }
}
