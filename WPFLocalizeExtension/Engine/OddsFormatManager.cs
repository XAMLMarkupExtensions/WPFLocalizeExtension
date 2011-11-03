using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

//////// Register this namespace under admirals one with prefix
////[assembly: XmlnsDefinition("http://schemas.root-project.org/xaml/presentation", "WPFLocalizeExtension.Engine")]
////[assembly: XmlnsDefinition("http://schemas.root-project.org/xaml/presentation", "WPFLocalizeExtension.Extensions")]
//////// Assign a default namespace prefix for the schema
////[assembly: XmlnsPrefix("http://schemas.root-project.org/xaml/presentation", "lex")]

namespace WPFLocalizeExtension.Engine
{
    /// <summary>
    /// Represents the odds format manager
    /// </summary>
    public sealed class OddsFormatManager : DependencyObject
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> DesignOddsFormat to set the <see cref="OddsFormatType"/>.
        /// Only supported at DesignTime.
        /// </summary>
        [DesignOnly(true)] public static readonly DependencyProperty DesignOddsFormatProperty =
            DependencyProperty.RegisterAttached(
                "DesignOddsFormat",
                typeof(OddsFormatType),
                typeof(OddsFormatManager),
                new PropertyMetadata(DefaultOddsFormatType, SetOddsFormatFromDependencyProperty));

        /// <summary>
        /// Holds a SyncRoot to be thread safe
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Holds the instance of singleton
        /// </summary>
        private static OddsFormatManager instance;

        /// <summary>
        /// Holds the current chosen <see cref="OddsFormatType"/>.
        /// </summary>
        private OddsFormatType oddsFormatType = DefaultOddsFormatType;

        /// <summary>
        /// Prevents a default instance of the <see cref="OddsFormatManager"/> class from being created. 
        /// Static Constructor
        /// </summary>
        private OddsFormatManager() {}

        /// <summary>
        /// Get raised if the <see cref="OddsFormatManager"/>.<see cref="OddsFormatType"/> is changed.
        /// </summary>
        internal event Action OnOddsFormatChanged;

        /// <summary>
        /// Gets the default <see cref="OddsFormatType"/> to initialize the 
        /// <see cref="OddsFormatManager"/>.<see cref="OddsFormatType"/>.
        /// </summary>
        public static OddsFormatType DefaultOddsFormatType
        {
            get { return OddsFormatType.EU; }
        }

        /// <summary>
        /// Gets the <see cref="OddsFormatManager"/> singleton.
        /// If the underlying instance is null, a instance will be created.
        /// </summary>
        public static OddsFormatManager Instance
        {
            get
            {
                // check if the underlying instance is null
                if (instance == null)
                {
                    // if it is null, lock the syncroot.
                    // if another thread is accessing this too, 
                    // it have to wait until the syncroot is released
                    lock (SyncRoot)
                    {
                        // check again, if the underlying instance is null
                        if (instance == null)
                        {
                            // create a new instance
                            instance = new OddsFormatManager();
                        }
                    }
                }

                // return the existing/new instance
                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the OddsFormatType for localization.
        /// On set, <see cref="OnOddsFormatChanged"/> is raised.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// You have to set <see cref="OddsFormatManager"/>.<see cref="OddsFormatType"/> first or 
        /// wait until System.Windows.Application.Current.MainWindow is created.
        /// Otherwise you will get an Exception.</exception>
        /// <exception cref="System.ArgumentNullException">thrown if OddsFormatType is not defined</exception>
        public OddsFormatType OddsFormatType
        {
            get { return this.oddsFormatType; }

            set
            {
                // the suplied value has to be defined, otherwise an exception will be raised
                if (!Enum.IsDefined(typeof(OddsFormatType), value))
                {
                    throw new ArgumentNullException("value");
                }

                // Set the OddsFormatType
                this.oddsFormatType = value;

                // Raise the OnOddsFormatChanged event
                if (this.OnOddsFormatChanged != null)
                {
                    this.OnOddsFormatChanged();
                }
            }
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> DesignOddsFormat.
        /// Only supported at DesignTime.
        /// If its in Runtime, the current <see cref="OddsFormatType"/> will be returned.
        /// </summary>
        /// <param name="obj">The dependency object to get the odds format type from.</param>
        /// <returns>The design odds format at design time or the current odds format at runtime.</returns>
        [DesignOnly(true)]
        public static OddsFormatType GetDesignOddsFormat(DependencyObject obj)
        {
            if (Instance.GetIsInDesignMode())
            {
                return (OddsFormatType) obj.GetValue(DesignOddsFormatProperty);
            }
            else
            {
                return Instance.OddsFormatType;
            }
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> DesignOddsFormat.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object to set the odds format to.</param>
        /// <param name="value">The odds format.</param>
        [DesignOnly(true)]
        public static void SetDesignOddsFormat(DependencyObject obj, OddsFormatType value)
        {
            if (Instance.GetIsInDesignMode())
            {
                obj.SetValue(DesignOddsFormatProperty, value);
            }
        }

        /// <summary>
        /// Attach an WeakEventListener to the <see cref="OddsFormatManager"/>
        /// </summary>
        /// <param name="listener">The listener to attach</param>
        public void AddEventListener(IWeakEventListener listener)
        {
            // calls AddListener from the inline WeakOddsFormatChangedEventManager
            WeakOddsFormatChangedEventManager.AddListener(listener);
        }

        /// <summary>
        /// Gets the status of the design mode
        /// </summary>
        /// <returns>TRUE if in design mode, else FALSE</returns>
        public bool GetIsInDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(this);
        }

        /// <summary>
        /// Detach an WeakEventListener to the <see cref="OddsFormatManager"/>
        /// </summary>
        /// <param name="listener">The listener to detach</param>
        public void RemoveEventListener(IWeakEventListener listener)
        {
            // calls RemoveListener from the inline WeakOddsFormatChangedEventManager
            WeakOddsFormatChangedEventManager.RemoveListener(listener);
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="OddsFormatManager"/>.<see cref="OddsFormatType"/> if set in Xaml.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object to set the odds format to.</param>
        /// <param name="args">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance 
        /// containing the event data.</param>
        [DesignOnly(true)]
        private static void SetOddsFormatFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!Instance.GetIsInDesignMode())
            {
                return;
            }

            if (!Enum.IsDefined(typeof(OddsFormatType), args.NewValue))
            {
                if (Instance.GetIsInDesignMode())
                {
                    Instance.OddsFormatType = DefaultOddsFormatType;
                }
                else
                {
                    throw new InvalidCastException(string.Format("\"{0}\" not defined in Enum OddsFormatType", args.NewValue));
                }
            }
            else
            {
                Instance.OddsFormatType = (OddsFormatType) Enum.Parse(typeof(OddsFormatType), args.NewValue.ToString(), true);
            }
        }

        /// <summary>
        /// This in line class is used to handle weak events to avoid memory leaks
        /// </summary>
        internal sealed class WeakOddsFormatChangedEventManager : WeakEventManager
        {
            /// <summary>
            /// Indicates, if the current instance is listening on the source event
            /// </summary>
            private bool isListening;

            /// <summary>
            /// Holds the inner list of listeners
            /// </summary>
            private ListenerList listeners;

            /// <summary>
            /// Prevents a default instance of the <see cref="WeakOddsFormatChangedEventManager"/> class from being created. 
            /// Creates a new instance of WeakOddsFormatChangedEventManager
            /// </summary>
            private WeakOddsFormatChangedEventManager()
            {
                // creates a new list and assign it to listeners
                this.listeners = new ListenerList();
            }

            /// <summary>
            /// Gets the singleton instance of <see cref="WeakOddsFormatChangedEventManager"/>
            /// </summary>
            private static WeakOddsFormatChangedEventManager CurrentManager
            {
                get
                {
                    // store the type of this WeakEventManager
                    Type managerType = typeof(WeakOddsFormatChangedEventManager);

                    // try to retrieve an existing instance of the stored type
                    WeakOddsFormatChangedEventManager manager =
                        (WeakOddsFormatChangedEventManager) GetCurrentManager(managerType);

                    // if the manager does not exists
                    if (manager == null)
                    {
                        // create a new instance of WeakOddsFormatChangedEventManager
                        manager = new WeakOddsFormatChangedEventManager();

                        // add the new instance to the WeakEventManager manager-store
                        SetCurrentManager(managerType, manager);
                    }

                    // return the new / existing WeakOddsFormatChangedEventManager instance
                    return manager;
                }
            }

            /// <summary>
            /// Adds an listener to the inner list of listeners
            /// </summary>
            /// <param name="listener">The listener to add</param>
            internal static void AddListener(IWeakEventListener listener)
            {
                // add the listener to the inner list of listeners
                CurrentManager.listeners.Add(listener);

                // start / stop the listening process
                CurrentManager.StartStopListening();
            }

            /// <summary>
            /// Removes an listener from the inner list of listeners
            /// </summary>
            /// <param name="listener">The listener to remove</param>
            internal static void RemoveListener(IWeakEventListener listener)
            {
                // removes the listener from the inner list of listeners
                CurrentManager.listeners.Remove(listener);

                // start / stop the listening process
                CurrentManager.StartStopListening();
            }

            /// <summary>
            /// This method starts the listening process by attaching on the source event
            /// </summary>
            /// <param name="source">The source.</param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override void StartListening(object source)
            {
                if (!this.isListening)
                {
                    Instance.OnOddsFormatChanged += this.Instance_OnOddsFormatChanged;
                    LocalizeDictionary.Instance.OnCultureChanged += this.Instance_OnCultureChanged;
                    this.isListening = true;
                }
            }

            /// <summary>
            /// This method stops the listening process by detaching on the source event
            /// </summary>
            /// <param name="source">The source to stop listening on.</param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override void StopListening(object source)
            {
                if (this.isListening)
                {
                    Instance.OnOddsFormatChanged -= this.Instance_OnOddsFormatChanged;
                    LocalizeDictionary.Instance.OnCultureChanged -= this.Instance_OnCultureChanged;
                    this.isListening = false;
                }
            }

            /// <summary>
            /// This method is called if the <see cref="LocalizeDictionary"/>.OnCultureChanged
            /// is called and the listening process is enabled
            /// </summary>
            private void Instance_OnCultureChanged()
            {
                // tells every listener in the list that the event is occurred
                this.DeliverEventToList(Instance, EventArgs.Empty, this.listeners);
            }

            /// <summary>
            /// This method is called if the <see cref="OddsFormatManager"/>.OnOddsFormatChanged
            /// is called and the listening process is enabled
            /// </summary>
            private void Instance_OnOddsFormatChanged()
            {
                // tells every listener in the list that the event is occurred
                this.DeliverEventToList(Instance, EventArgs.Empty, this.listeners);
            }

            /// <summary>
            /// This method starts and stops the listening process by attaching/detaching on the source event
            /// </summary>
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void StartStopListening()
            {
                // check if listeners are available and the listening process is stopped, start it.
                // otherwise if no listeners are available and the listening process is started, stop it
                if (this.listeners.Count != 0)
                {
                    if (!this.isListening)
                    {
                        this.StartListening(null);
                    }
                }
                else
                {
                    if (this.isListening)
                    {
                        this.StopListening(null);
                    }
                }
            }
        }
    }
}