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
using System.Reflection;
using System.Collections.Generic;
using Org.Mentalis.Proxy.Socks.Authentication;

namespace Org.Mentalis.Proxy
{
    
   
    /// <summary>
    /// Defines the class that controls the settings and listener objects.
    /// </summary>
    public class Proxy:IProxy
    {
        /// <summary>
        /// Initializes a new Proxy instance.
        /// </summary>
        public Proxy()
        {
            Config = new ProxyConfig(this, new AuthenticationList());
            listeners = new Dictionary<Guid, Listener>();
        }

        /// <summary>
        /// Stops the proxy server.
        /// </summary>
        /// <remarks>When this method is called, all listener and client objects will be disposed.</remarks>
        public void Stop()
        {
            foreach (var listener in listeners.Values)
            {
                OnListenerStopped(new ListenerEventArgs() { Listener = listener });
                listener.Dispose();
            }
            listeners.Clear();
        }

        public void SaveData(string filename)
        {
            Config.SaveData(filename);
        }



        public void LoadData(string filename)
        {
            Config.LoadData(filename);
        }


        public IDictionary<Guid, Listener> ListListeners()
        {
            return new Dictionary<Guid, Listener>(listeners);
        }

        public void RemoveListenerById(Guid id)
        {
            Listener listener;
            if (!listeners.TryGetValue(id, out listener))
                throw new ArgumentOutOfRangeException(string.Format("Listener with id={0} not found", id), "id");
            OnListenerStopped(new ListenerEventArgs() { Listener = listener });
            listener.Dispose();
            listeners.Remove(id);
        }
        /// <summary>
        /// Adds a listener to the Listeners list.
        /// </summary>
        /// <param name="newItem">The new Listener to add.</param>

        public void AddListener(Type type, object[] pars)
        {
            AddListener(Guid.NewGuid(), type, pars);
        }

        public void AddListener(Guid id, Type type, object[] pars)
        {
            var listener = (Listener)Activator.CreateInstance(type, pars);
            AddListener(id, listener);
        }

        public void AddListener(Listener newItem)
        {
            AddListener(Guid.NewGuid(), newItem);
        }

        private void AddListener(Guid id, Listener newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException();
            listeners[id] = newItem;
            OnListenerStarted(new ListenerEventArgs {Listener = newItem});
            newItem.Start();
        }

        public bool IsUserPresent(string name)
        {
            return Config.UserList.IsUserPresent(name);
        }

        public void AddUser(string name, string pass1)
        {
            Config.UserList.AddItem(name, pass1);
            OnUserCreated(new UserEventArgs {Username = name});
        }

        public void RemoveUser(string name)
        {
            Config.UserList.RemoveItem(name);
            OnUserDeleted(new UserEventArgs { Username = name });
        }

        public IList<string> GetUserNames()
        {
            return Config.UserList.Usernames;
        }


        /// <summary>
        /// Gets or sets the configuration object for this Proxy server.
        /// </summary>
        /// <value>A ProxyConfig instance that represents the configuration object for this Proxy server.</value>
        public ProxyConfig Config { get; private set; }

        private readonly IDictionary<Guid, Listener> listeners;


        public event EventHandler<ListenerEventArgs> ListenerStarted;
        public event EventHandler<ListenerEventArgs> ListenerStopped;
        public event EventHandler<UserEventArgs> UserCreated;
        public event EventHandler<UserEventArgs> UserDeleted;


        protected void OnListenerStarted(ListenerEventArgs e)
        {
            var handler = ListenerStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnListenerStopped(ListenerEventArgs e)
        {
            var handler = ListenerStopped;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnUserCreated(UserEventArgs e)
        {
            var handler = UserCreated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnUserDeleted(UserEventArgs e)
        {
            var handler = UserDeleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }

    public class ListenerEventArgs:EventArgs
    {
        public Listener Listener { get; set; }
    }

    public class UserEventArgs:EventArgs
    {
        public string Username { get; set; }
    }
}