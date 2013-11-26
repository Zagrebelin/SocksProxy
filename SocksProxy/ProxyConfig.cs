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
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Text;
using System.Collections.Specialized;
using System.Reflection;
using Org.Mentalis.Proxy.Socks.Authentication;
using SocksProxy;

namespace Org.Mentalis.Proxy
{
    /// <summary>
    /// Stores the configuration settings of this proxy server.
    /// </summary>
    public sealed class ProxyConfig
    {
        /// <summary>
        /// Initializes a new ProxyConfig instance.
        /// </summary>
        /// <param name="parent">The parent of this ProxyCondif instance.</param>
        /// <exception cref="ArgumentNullException"><c>file</c> is null -or- <c>parent</c> is null.</exception>
        public ProxyConfig(IProxy parent, IAuthenticationList mUserList)
        {
            if (parent == null)
                throw new ArgumentNullException();
            m_Parent = parent;
            m_UserList = mUserList;
        }

        #region Save config
        /// <summary>
        /// Saves the data in this class to an XML file.
        /// </summary>
        public void SaveData()
        {
            var cc = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var ss = cc.Sections["mentalis"] as MentalisSection;
            if (ss != null)
            {
                ss.Users.Clear();
                foreach (var user in UserList.Listing.Keys.OfType<string>())
                {
                    ss.Users.Add(new MentalisSection.UsersElementCollection.UserElement
                                     {
                                         Name = user,
                                         Hash = UserList.Listing[user]
                                     });
                }
                ss.Listeners.Clear();
                var ls = Parent.ListListeners().Select(p => new {id = p.Key, prms = p.Value.ConstructString, type=p.Value.GetType().AssemblyQualifiedName});
                foreach (var listener in ls)
                {
                    ss.Listeners.Add(new MentalisSection.ListenersElementCollection.ListenerElement
                                         {
                                             Id = listener.id.ToString(),
                                             Type = listener.type,
                                             Params = listener.prms
                                         });
                }
            }
            cc.Save();

        }
        #endregion

        #region Load config

        /// <summary>
        /// Loads the data from an XML file.
        /// </summary>
        public void LoadData()
        {
            var c = Mentalis.Config;
            if (c == null)
                return;

            foreach (MentalisSection.UsersElementCollection.UserElement user in c.Users)
            {
                UserList.AddUserWithCryptedPassword(user.Name, user.Hash);
            }

            foreach (MentalisSection.ListenersElementCollection.ListenerElement listener in c.Listeners)
            {
                var klass = Type.GetType(listener.Type);
                Guid id;
                if (!Guid.TryParse(listener.Id, out id))
                    continue;
                if (klass != null)
                    Parent.AddListener(id, klass, Tools.ParseStringToParams(listener.Params));
            }
        }
        #endregion
        
        /// <summary>
        /// Gets the parent object of this ProxyConfig class.
        /// </summary>
        /// <value>An instance of the Proxy class.</value>
        public IProxy Parent
        {
            get
            {
                return m_Parent;
            }
        }
        /// <summary>
        /// Gets the userlist.
        /// </summary>
        /// <value>An instance of the AuthenticationList class that holds all the users and their corresponding password hashes.</value>
        public IAuthenticationList UserList
        {
            get
            {
                return m_UserList;
            }
        }
        // private variables
        /// <summary>Holds the value of the UserList property.</summary>
        private readonly IAuthenticationList m_UserList;
        /// <summary>Holds the value of the Parent property.</summary>
        private readonly IProxy m_Parent;
    }

}
/*
<MentalisProxy>
  <Version value="1.0" />
  <Settings>
    <authorize value="never" />
    <admin_hash value="Gh3JHJBzJcaScd3wyUS8cg==" />
    <config_ip value="localhost" />
    <config_port value="4" />
    <errorlog value="errors.log" />
  </Settings>
  <Users>
    <user value="myhash" />
    <pieter value="WhBei51A4TKXgNYuoiZdig==" />
    <kris value="X03MO1qnZdYdgyfeuILPmQ==" />
  </Users>
  <Listeners>
    <listener type="Org.Mentalis.Proxy.Http.HttpListener" value="host:0.0.0.0;int:100" />
    <listener type="Org.Mentalis.Proxy.Ftp.FtpListener" value="host:0.0.0.0;int:21" />
    <listener type="Org.Mentalis.Proxy.Socks.SocksListener" value="host:0.0.0.0;int:1080;authlist" />
    <listener type="Org.Mentalis.Proxy.PortMap.PortMapListener" value="host:0.0.0.0;int:12345;host:msnews.microsoft.com;int:119" />
  </Listeners>
</MentalisProxy>
*/
