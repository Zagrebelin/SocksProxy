using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Org.Mentalis.Proxy
{
    public class AvailableListenerManager
    {
        private IDictionary<string, Type> availableListeners;

        public AvailableListenerManager()
        {
            availableListeners = new Dictionary<String, Type>();
            foreach (var t in System.Reflection.Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                if (t.IsSubclassOf(typeof(Listener)))
                {
                    var name = t.Name.Remove(t.Name.Length - 8).ToLower();
                    var idx = 0;
                    var prefix = name;
                    while (availableListeners.ContainsKey(name))
                    {
                        idx += 1;
                        name = string.Format("{0}_{1}", prefix, idx);
                    }

                    availableListeners.Add(name, t);
                }
            }
        }

        public void DoAll(Action<String, Type> action)
        {
            foreach (var id in availableListeners.Keys)
            {
                action(id, availableListeners[id]);
            }
        }

        internal bool ContainsKey(string classtype)
        {
            return availableListeners.ContainsKey(classtype);
        }

        internal string GetFullName(string classtype)
        {
            return availableListeners[classtype].FullName;
        }
    }
}
