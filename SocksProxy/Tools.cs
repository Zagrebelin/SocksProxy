using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SocksProxy
{
    public static class Tools
    {
        public static object[] ParseStringToParams(string cpars)
        {
            string[] parts = cpars.Split(';');
            var pars = new List<object>();
            string oval = null;
            // Start instantiating the objects to give to the constructor
            foreach (var part in parts)
            {
                int ret = part.IndexOf(':');
                string otype = null;
                if (ret >= 0)
                {
                    otype = part.Substring(0, ret);
                    oval = part.Substring(ret + 1);
                }
                else
                {
                    otype = part;
                }
                switch (otype.ToLower())
                {
                    case "int":
                        pars.Add(int.Parse(oval));
                        break;
                    case "host":
                        pars.Add(Dns.Resolve(oval).AddressList[0]);
                        break;
                    case "authlist":
                        //                            pars.Add(_proxy.Config.UserList);
                        break;
                    case "null":
                        pars.Add(null);
                        break;
                    case "string":
                        pars.Add(oval);
                        break;
                    case "ip":
                        pars.Add(IPAddress.Parse(oval));
                        break;
                    default:
                        pars.Add(null);
                        break;
                }
            }
            return pars.ToArray();
            
        }
    }
}
