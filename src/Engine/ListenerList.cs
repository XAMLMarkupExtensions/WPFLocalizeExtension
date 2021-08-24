namespace WPFLocalizeExtension.Engine
{
    #region Usings
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    #endregion
    
    /// <summary>
    /// Represents a collection of listeners.
    /// </summary>
    internal class ListenersList
    {
        private readonly Dictionary<WeakReference, int> listeners;
        private readonly Dictionary<int, List<WeakReference>> listenersHashCodes;
        private readonly List<WeakReference> deadListeners;

        /// <summary>
        /// Create new empty <see cref="ListenersList" /> instance.
        /// </summary>
        public ListenersList()
        {
            listeners = new Dictionary<WeakReference, int>();
            listenersHashCodes = new Dictionary<int, List<WeakReference>>();
            deadListeners = new List<WeakReference>();
        }

        /// <summary>
        /// The count of listeners.
        /// </summary>
        public int Count => listeners.Count;

        /// <summary>
        /// Add new listener.
        /// </summary>
        public void AddListener(IDictionaryEventListener listener)
        {
            ClearDeadReferences();
            
            // Add listener if it not registered yet.
            var weakReference = new WeakReference(listener);
            var hashCode = listener.GetHashCode();
            if (!listenersHashCodes.TryGetValue(hashCode, out var sameHashCodeListeners))
            {
                listeners.Add(weakReference, hashCode);
                listenersHashCodes.Add(hashCode, new List<WeakReference> { weakReference });
            }
            else if (sameHashCodeListeners.All(wr => wr.Target != listener))
            {
                listeners.Add(weakReference, hashCode);
                sameHashCodeListeners.Add(weakReference);
            }
        }

        /// <summary>
        /// Get all alive listeners.
        /// </summary>
        public IEnumerable<IDictionaryEventListener> GetListeners()
        {
            ClearDeadReferences();
                
            foreach (var listener in listeners)
            {
                var listenerReference = listener.Key.Target as IDictionaryEventListener;
                if (listenerReference == null)
                {
                    deadListeners.Add(listener.Key);
                    continue;
                }

                yield return listenerReference;
            }
        }

        /// <summary>
        /// Remove listener.
        /// </summary>
        public void RemoveListener(IDictionaryEventListener listener)
        {
            ClearDeadReferences();
            
            var hashCode = listener.GetHashCode();
            if (!listenersHashCodes.TryGetValue(hashCode, out var hashCodes))
                return;

            var wr = hashCodes.FirstOrDefault(l => l.Target == listener);
            if (wr == null)
                return;

            if (hashCodes.Count > 1)
                hashCodes.Remove(wr);
            else
                listenersHashCodes.Remove(hashCode);

            listeners.Remove(wr);
        }

        /// <summary>
        /// Clear internal list from all dead listeners.
        /// </summary>
        private void ClearDeadReferences()
        {
            if (deadListeners.Count == 0)
                return;
            
            foreach (var deadListener in deadListeners)
            {
                var hashCode = listeners[deadListener];
                listenersHashCodes[hashCode].Remove(deadListener);
                if (!listenersHashCodes[hashCode].Any())
                    listenersHashCodes.Remove(hashCode);

                listeners.Remove(deadListener);
            }

            deadListeners.Clear();
        }
    }
}