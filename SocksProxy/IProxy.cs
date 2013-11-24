using System;
using System.Collections.Generic;

namespace Org.Mentalis.Proxy
{
    public interface IProxy
    {
        event EventHandler<ListenerEventArgs> ListenerStarted;
        event EventHandler<ListenerEventArgs> ListenerStopped;
        event EventHandler<UserEventArgs> UserCreated;
        event EventHandler<UserEventArgs> UserDeleted;

        void LoadData(string filename);
        void SaveData(string filename);

        void AddListener(Listener listener);
        IDictionary<Guid, Listener> ListListeners();
        void RemoveListenerById(Guid id);
        void AddListener(Type type, object[] pars);
        void AddListener(Guid id, Type klass, object[] p);
        
        bool IsUserPresent(string name);
        void AddUser(string name, string pass1);
        void RemoveUser(string name);
        IList<string> GetUserNames();
        
        void Stop();
    }
}