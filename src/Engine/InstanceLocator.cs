using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WPFLocalizeExtension.Engine
{
    internal static class InstanceLocator
    {
        /// <summary>
        /// Holds a SyncRoot to be thread safe
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Holds the threaded instances per type
        /// </summary>
        private static Dictionary<Type, Dictionary<int, object>> _instances = new Dictionary<Type, Dictionary<int, object>>();

        /// <summary>
        /// Get a singleton or per thread instance
        /// </summary>
        /// <typeparam name="T">Type of the requested instance. Has to implement a public parameterless constructor.</typeparam>
        /// <returns>instance of type <typeparamref name="T"/></returns>
        public static T Resolve<T>() where T : class, new()
        {
            var instances = GetTypeInstances<T>();
            return GetInstance<T>(instances);
        }

        /// <summary>
        /// Get a singleton or per thread instance
        /// </summary>
        /// <param name="type">Type of the requested instance. Has to implement a public parameterless constructor.</param>
        /// <returns>instance of <paramref name="type"/></returns>
        public static object Resolve(Type type)
        {
            var instances = GetTypeInstances(type);
            return GetInstance(instances, type);
        }

        /// <summary>
        /// Get the instance dictionary for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">instance type</typeparam>
        /// <returns>The new / existing instance collection for the requested type</returns>
        public static Dictionary<int, object> GetTypeInstances<T>() where T : class
        {
            var type = typeof(T);
            return GetTypeInstances(type);
        }

        /// <summary>
        /// Get the instance dictionary for <paramref name="type"/>
        /// </summary>
        /// <param name="type">instance type</param>
        /// <returns>The new / existing instance collection for the requested type</returns>
        public static Dictionary<int, object> GetTypeInstances(Type type)
        {
            if (!_instances.ContainsKey(type))
            {
                lock (SyncRoot)
                {
                    _instances.Add(type, new Dictionary<int, object>());
                }
            }

            return _instances[type];
        }

        /// <summary>
        /// Get a instance of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">instance type</typeparam>
        /// <param name="instances">instances dictionary</param>
        /// <returns>The new / existing instance of type <typeparamref name="T"/></returns>
        private static T GetInstance<T>(Dictionary<int, object> instances) where T : class, new()
        {
            return GetInstance(instances, typeof(T)) as T;
        }

        /// <summary>
        /// Get a instance of <paramref name="type"/>
        /// </summary>
        /// <param name="instances">instances dictionary</param>
        /// <param name="type">instance type</param>
        /// <returns>The new / existing instance of type <paramref name="type"/></returns>
        private static object GetInstance(Dictionary<int, object> instances, Type type)
        {
            object result = null;
            var threadId = Thread.CurrentThread.ManagedThreadId;

            // when UseThreadInstances is activated the requested type has to implement the ILocalizeInstance interface
            // otherwise we will use a singleton instance
            if (LocalizeSettings.Instance.UseThreadInstances && typeof(ILocalizeInstance).IsAssignableFrom(type))
            {
                if (instances.ContainsKey(threadId))
                    result = instances[threadId];
            }
            else
                result = instances.FirstOrDefault().Value;

            if (result == null)
            {
                lock (SyncRoot)
                {
                    result = Activator.CreateInstance(type);
                    CheckShutdownEvent(Thread.CurrentThread);
                    instances.Add(threadId, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Check if the provided thread populates a dispatcher object, subsribe to the <see cref="Dispatcher.ShutdownStarted"/> event and dissolve instances for this thread.
        /// </summary>
        /// <param name="thread"></param>
        private static void CheckShutdownEvent(Thread thread)
        {
            var dis = Dispatcher.FromThread(thread);
            if (dis != null && !_instances.Any(x => x.Value.Any(y => y.Key == thread.ManagedThreadId)))
            {
                dis.ShutdownStarted += (o, e) =>
                {
                    foreach (var entry in _instances.SelectMany(x => x.Value.Where(y => y.Key == thread.ManagedThreadId)).ToList())
                        Dissolve(entry.Value);
                };
            }
        }

        /// <summary>
        /// Remove a instance from the collection
        /// </summary>
        /// <param name="obj">instance</param>
        internal static void Dissolve(object obj)
        {
            var instances = GetTypeInstances(obj.GetType());
            if (instances.Any(x => x.Value == obj))
            {
                var entry = instances.Single(x => x.Value == obj);
                lock (SyncRoot)
                {
                    instances.Remove(entry.Key);
                }
            }
        }
    }

    /// <summary>
    /// A empty interface - just to indicate a class is useable for the thread / instance logic
    /// </summary>
    public interface ILocalizeInstance { }
}
