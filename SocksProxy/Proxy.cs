/*
    Copyright © 2002, The KPD-Team
    All rights reserved.
    http://www.mentalis.org/

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions
  are met:

    - Redistributions of source code must retain the above copyright
       notice, this list of conditions and the following disclaimer. 

    - Neither the name of the KPD-Team, nor the names of its contributors
       may be used to endorse or promote products derived from this
       software without specific prior written permission. 

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
  THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
  STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
  OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Net.Sockets;
using System.Security.Cryptography;
using Org.Mentalis.Proxy;
using Org.Mentalis.Proxy.Ftp;
using Org.Mentalis.Proxy.Http;
using Org.Mentalis.Proxy.Socks;
using Org.Mentalis.Proxy.PortMap;
using Org.Mentalis.Proxy.Socks.Authentication;
using Org.Mentalis.Utilities.ConsoleAttributes;
using System.Collections.Generic;

namespace Org.Mentalis.Proxy
{
    
    public struct ProxyCommand
    {
        public Action Action;
        public string HelpString;
    }
    /// <summary>
    /// Defines the class that controls the settings and listener objects.
    /// </summary>
    public class Proxy
    {
        /// <summary>
        /// Initializes a new Proxy instance.
        /// </summary>
        public Proxy()
        {
            Config = new ProxyConfig(this);
            commands = new Dictionary<String, ProxyCommand>{
                {"help", new ProxyCommand{Action=ShowHelp, HelpString="Shows this help message"}},
                {"uptime", new ProxyCommand{Action=ShowUpTime, HelpString="Shows the uptime of the proxy server"}},
                {"version", new ProxyCommand{Action=ShowVersion, HelpString="Prints the version of this program"}},
                {"listusers", new ProxyCommand{Action=()=>UserManager.ShowUsers(Config), HelpString="Lists all users"}},
                {"adduser", new ProxyCommand{Action=()=>UserManager.AddUser(Config), HelpString="Adds a user to the user list"}},
                {"deluser", new ProxyCommand{Action=()=>UserManager.DeleteUser(Config), HelpString="Deletes a user from the user list"}},
                {"listlisteners", new ProxyCommand{Action=ShowListeners, HelpString="Lists all the listeners"}},
                {"addlistener", new ProxyCommand{Action=ShowAddListener, HelpString="Adds a new listener"}},
                {"dellistener", new ProxyCommand{Action=ShowDelListener, HelpString="Deletes a listener"}},
                {"listeners", new ProxyCommand{Action=ShowAvailableListeners, HelpString="List available listeners"}},
                {"exit", new ProxyCommand{Action=()=>{}, HelpString="Exit the application"}},
                {"save", new ProxyCommand{Action=()=>Config.SaveData("xxx"), HelpString="Save configuration"}},
            };

            
        }
        
        /// <summary>
        /// Asks the user which listener to delete.
        /// </summary>
        protected void ShowDelListener()
        {
            Console.WriteLine("Please enter the ID of the listener you want to delete:\r\n (use the 'listlisteners' command to show all the listener IDs)");
            string id = Console.ReadLine();
            try
            {
                listenerManager.Remove(id);
                Console.WriteLine("Listener removed from the list.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Specified ID not found in list!");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid ID tag!");
            }
        }
                
    

        /// <summary>
        /// Shows the Listeners list.
        /// </summary>
        protected void ShowListeners()
        {
            listenerManager.DoAll((guid, listener) =>
            {
                Console.WriteLine(listener.ToString());
                Console.WriteLine("  id: {0}", guid.ToString("N"));
            });
        }

        /// <summary>
        /// Asks the user which listener to add.
        /// </summary>
        protected void ShowAddListener()
        {
            Console.WriteLine(
                "Please enter the full class name of the Listener object you're trying to add:\r\n (ie. Org.Mentalis.Proxy.Http.HttpListener) or short name from table of Available Listeners:");
            ShowAvailableListeners();
            string classtype = Console.ReadLine();
            if (classtype == "")
                return;
            else if (availableManager.ContainsKey(classtype))
            {
                classtype = availableManager.GetFullName(classtype);
            }
            else if (Type.GetType(classtype) == null)
            {
                Console.WriteLine("The specified class does not exist!");
                return;
            }
            Console.WriteLine("Please enter the construction parameters:");
            ShowAvailableConstructors(classtype);
            string construct = Console.ReadLine();
            object listenObject = CreateListener(classtype, construct);
            if (listenObject == null)
            {
                Console.WriteLine("Invalid construction string.");
                return;
            }
            Listener listener;
            try
            {
                listener = (Listener) listenObject;
            }
            catch
            {
                Console.WriteLine("The specified object is not a valid Listener object.");
                return;
            }
            try
            {
                listener.Start();
                AddListener(listener);
            }
            catch
            {
                Console.WriteLine("Error while staring the Listener.\r\n(Perhaps the specified port is already in use?)");
                return;
            }
        }

        private void ShowAvailableConstructors(string classtype)
        {
            var type = Type.GetType(classtype);
            var constructors = type.GetConstructors();
            foreach (var info in constructors)
            {
                var pars = info.GetParameters();
                var spars = pars.Select(par => string.Format("{0}:{1}", par.ParameterType.Name, par.Name));
                Console.WriteLine(string.Join(";", spars));
            }
        }

        /// <summary>
        /// Shows a list of commands in the console.
        /// </summary>
        protected void ShowHelp()
        {
            foreach (var command in commands.Keys)
            {
                Console.WriteLine("{0} - {1}", command, commands[command].HelpString);
            }
            Console.WriteLine("\r\n Read the readme.txt file for more help.");
        }
        /// <summary>
        /// Shows the uptime of this proxy server.
        /// </summary>
        protected void ShowUpTime()
        {
            TimeSpan uptime = DateTime.Now.Subtract(StartTime);
            Console.WriteLine("Up " + uptime.ToString());
        }
        /// <summary>
        /// Shows the version number of this proxy server.
        /// </summary>
        protected void ShowVersion()
        {
            Console.WriteLine("This is version " + Assembly.GetCallingAssembly().GetName().Version.ToString(3) + " of the Mentalis.org proxy server.");
        }

        protected void ShowAvailableListeners()
        {
            availableManager.DoAll((id, type) => Console.WriteLine("{0} -> {1}", id, type));
        }
        


        /// <summary>
        /// Stops the proxy server.
        /// </summary>
        /// <remarks>When this method is called, all listener and client objects will be disposed.</remarks>
        public void Stop()
        {
            // Stop listening and clear the Listener list
            listenerManager.DoAll(le =>
            {
                Console.WriteLine(le.ToString() + " stopped");
                le.Dispose();
            });
            
            listenerManager.RemoveAll();
        }
        /// <summary>
        /// Adds a listener to the Listeners list.
        /// </summary>
        /// <param name="newItem">The new Listener to add.</param>
        public void AddListener(Listener newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException();
            listenerManager.AddListener(newItem);
            Console.WriteLine(newItem.ToString() + " started.");
        }
        /// <summary>
        /// Creates a new Listener obejct from a given listener name and a given listener parameter string.
        /// </summary>
        /// <param name="type">The type of object to instantiate.</param>
        /// <param name="cpars"></param>
        /// <returns></returns>
        public Listener CreateListener(string type, string cpars)
        {
            try
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
                            pars.Add(Config.UserList);
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
                return (Listener)Activator.CreateInstance(Type.GetType(type), pars);
            }
            catch
            {
                return null;
            }
        }
        
       
        /// <summary>
        /// Gets or sets the date when this Proxy server was first started.
        /// </summary>
        /// <value>A DateTime structure that indicates when this Proxy server was first started.</value>
        protected DateTime StartTime { get; private set; }
        /// <summary>
        /// Gets or sets the configuration object for this Proxy server.
        /// </summary>
        /// <value>A ProxyConfig instance that represents the configuration object for this Proxy server.</value>
        protected ProxyConfig Config { get; private set; }
        /// <summary>All available commands</summary>
        private IDictionary<String, ProxyCommand> commands;

        public ListenerManager listenerManager = new ListenerManager();
        private AvailableListenerManager availableManager = new AvailableListenerManager();

        public void LoadData(string filename)
        {
            Config.LoadData(filename);
        }
    }
}