using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Org.Mentalis.Proxy
{
    public class ListenerManager
    {
        private IDictionary<Guid, Listener> listeners;

        public ListenerManager()
        {
            listeners = new Dictionary<Guid, Listener>();
            
        }

        
        internal void Remove(string id)
        {
            Guid guid;
            if (!Guid.TryParse(id, out guid))
            {
                throw new ArgumentException(id);
            }
            if (!listeners.ContainsKey(guid))
            {
                throw new ArgumentOutOfRangeException("id", "id not found");
            }
            listeners[guid].Dispose();
            listeners.Remove(guid);
        }

        internal void AddListener(Listener newItem)
        {
            listeners[Guid.NewGuid()] = newItem;
        }

        public void DoAll(Action<Listener> action)
        {
            foreach (var le in listeners.Values)
            {
                action(le);
            }
        }

        public void DoAll(Action<Guid, Listener> action)
        {
            foreach (var id in listeners.Keys)
            {
                action(id, listeners[id]);
            }
        }

        

        internal void RemoveAll()
        {
            listeners.Clear();
        }
    }
}
