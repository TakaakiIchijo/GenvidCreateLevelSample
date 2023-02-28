using UnityEngine;
using System.Collections;
using GenvidSDKCSharp;
using System;

namespace Genvid
{
    namespace Plugin
    {
        /// <summary>
        /// The session manager is responsible for driving
        /// the events of its sessions.
        /// </summary>
        [Serializable]
        public class SessionManager : IGenvidPlugin
        {
            /// <summary>
            /// The session manages all components necessary for a complete
            /// streaming experience: audio, video, data streams, commands, and events.
            /// </summary>
            [SerializeField]
            public Session Session;

            /// <summary>
            /// Keeps track of the session state.
            /// Toggled on when initialization succeeds.
            /// Toggled off when the session terminates.
            /// </summary> 
            public bool IsInitialized { get; private set; }

            /// <summary>
            /// Toggled on when the SDK initialization succeeds.
            /// </summary> 
            public bool IsSDKInitialized { get; private set; }

            /// <summary>
            /// Specifies logging verbosity. Toggle on for more logging info.
            /// </summary>
            public bool VerboseLog { get; set; }

            /// <summary>
            /// Initializes the SDK and the session.
            /// </summary>
            /// <returns>True if all components initialize correctly, false otherwise.</returns>
            public bool Initialize()
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (!IsInitialized)
                {
                    GenvidSDK.Status gvStatus = GenvidSDK.Initialize();
                    if (GenvidSDK.StatusFailed(gvStatus))
                    {
                        Debug.LogError("Error while initializing GenvidSDK: " + GenvidSDK.StatusToString(gvStatus));
                        return false;
                    }

                    Debug.Log("GenvidSDK.Initialize() performed correctly.");
                    IsSDKInitialized = true;
                    IsInitialized = Session.Initialize();
                }

                return IsInitialized;
#else
                return true;
#endif
            }

            /// <summary>
            /// Terminates the session and SDK.
            /// </summary>
            /// <returns>True if all components terminate correctly, false otherwise.</returns>
            public bool Terminate()
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (IsSDKInitialized)
                {
                    if (Session != null)
                    {
                        IsInitialized = Session.Terminate();
                    }

                    var gvStatus = GenvidSDK.Terminate();
                    if (GenvidSDK.StatusFailed(gvStatus))
                    {
                        Debug.LogError("Error while running the terminate process: " + GenvidSDK.StatusToString(gvStatus));
                        return false;
                    }

                    Debug.Log("GenvidSDK.Terminate() performed correctly.");
                }
                return IsInitialized;
#else
                return true;
#endif
            }

            /// <summary>
            /// Starts the session.
            /// Called after initialization and before the first update.
            /// </summary>
            public void Start()
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (IsInitialized)
                {
                    Session.Start();
                }
#endif
            }

            /// <summary>
            /// Evaluates if events have been received and updates the session.
            /// Called once per frame.
            /// </summary>
            public void Update()
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (IsInitialized)
                {
                    var gvStatus = GenvidSDK.CheckForEvents();
                    if ((GenvidSDK.StatusFailed(gvStatus)) && gvStatus != GenvidSDK.Status.ConnectionTimeout)
                    {
                        Debug.LogError("Error while running CheckForEvents: " + GenvidSDK.StatusToString(gvStatus));
                    }
                    else if (VerboseLog)
                    {
                        Debug.Log("Genvid CheckForEvents performed correctly.");
                    }

                    Session.Update();
                }
#endif
            }
        }
    }

    /// <summary>
    /// The session manager is responsible for driving
    /// the events of its sessions.
    /// </summary>
    [System.Obsolete("Deprecated. Call `Genvid.Plugin.SessionManager` to use Genvid session manager.")]
    public class GenvidSessionManager : MonoBehaviour
    {
        /// <summary>
        /// The session manager state selection.
        /// </summary>
        private enum State
        {
            Uninitialized,
            Initializing,
            Initialized,
            Destroying,
            Destroyed
        }

        /// <summary>
        /// Uses user input to know if the SDK should be active.
        /// </summary>
        public bool ActivateSDK = true;

        /// <summary>
        /// Toggle on to initialize the session as soon as the
        /// session manager monobehaviour is active.
        /// </summary>
        public bool AutoInitialize = true;

        /// <summary>
        /// The session manages all components necessary for a complete
        /// streaming experience: audio, video, data streams, commands, and events.
        /// </summary>
        public GenvidSession Session;

        /// <summary>
        /// Specifies logging verbosity. Toggle on for more logging info.
        /// </summary>
        public bool ActivateDebugLog = false;


#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
    // Disable warning for other platforms.
#pragma warning disable 414
#endif

        /// <summary>
        /// The session manager instance. There should be only one.
        /// </summary>
        private static GenvidSessionManager m_Instance;

        /// <summary>
        /// Locks the session manager initialization.
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// The session manager state.
        /// </summary>
        private static State m_State = State.Uninitialized;

        /// <summary>
        /// Toggles on when the session is created.
        /// Toggles off when the session is destroyed.
        /// </summary>
        private bool m_IsCreated = false;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
#pragma warning restore 414
#endif

        /// <summary>
        /// True when the session manager is destroyed.
        /// </summary>
        public static bool IsDestroyed { get { return m_State == State.Destroyed; } }

        /// <summary>
        /// True while the session manager is in the process of being destroyed.
        /// </summary>
        public static bool IsDestroying { get { return m_State == State.Destroying; } }

        /// <summary>
        /// True when the session manager is initialized and ready to function.
        /// </summary>
        public static bool IsInitialized { get { return m_State == State.Initialized; } }

        /// <summary>
        /// Special property used to disable video-data submission throttling.
        /// By default, the video is submitted at the framerate set in GenvidVideo.
        /// By disabling this property, the video submission will follow the game framerate.
        /// </summary>
        public static bool DisableVideoDataSubmissionThrottling { get; private set; }

        /// <summary>
        /// Returns a session manager singleton, persistent across scenes.
        /// </summary>
        /// <returns>A static session maanger instance.</returns>
        public static GenvidSessionManager Instance
        {
            get
            {
                if (IsDestroyed)
                {
                    return null;
                }

                lock (_lock)
                {
                    if (m_Instance == null)
                    {
                        var instances = FindObjectsOfType<GenvidSessionManager>();
                        m_Instance = FindObjectOfType<GenvidSessionManager>();

                        if (instances.Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                            " - there should never be more than 1 singleton!" +
                                            "Reopening the scene might fix it.");
                            return m_Instance;
                        }

                        if (m_Instance == null)
                        {
                            GameObject singleton = new GameObject();
                            m_Instance = singleton.AddComponent<GenvidSessionManager>();
                            singleton.name = "(singleton) " + typeof(GenvidSessionManager).ToString();


                            DontDestroyOnLoad(singleton);

                            Debug.Log("[Singleton] An instance of " + typeof(GenvidSessionManager) +
                                        " is needed in the scene, so '" + singleton +
                                        "' was created with DontDestroyOnLoad.");
                        }
                        else
                        {
                            DontDestroyOnLoad(m_Instance.gameObject);
                            Debug.Log("[Singleton] Using instance already created: " + m_Instance.gameObject.name);
                        }
                    }

                    return m_Instance;
                }
            }
        }

        /// <summary>
        /// Initializes the SDK and the session.
        /// </summary>
        public void Initialize()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            DisableVideoDataSubmissionThrottling = false;

            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-Genvid")
                {
                    ActivateSDK = true;
                }
                else if (args[i] == "-DisableVideoDataSubmissionThrottling")
                {
                    Debug.Log("Using 'DisableVideoDataSubmissionThrottling' property.");
                    Debug.Log("Genvid video submission will now follow the game framerate.");
                    DisableVideoDataSubmissionThrottling = true;
                }
            }

            if (ActivateSDK && !IsInitialized)
            {
                // 32 bits -> x86
                // 64 bits -> x86_64

                string dllPath = "/Plugins";
                string editorDllPath = "/Genvid/SDK/Plugins";
                string[] paths = { "", "/x86" };
                uint pathIndex = 0;
                bool arch86_64 = (IntPtr.Size == 8);

                while (true)
                {
                    if (Application.isEditor)
                    {
                        dllPath = editorDllPath + (arch86_64 ? "/x64" : "/x86");
                    }
                    else if (paths[pathIndex].Length != 0)
                    {
                        dllPath += arch86_64 ? (paths[pathIndex] + "_64") : paths[pathIndex];
                    }

                    if (!GenvidSDK.LoadGenvidDll(Application.dataPath + dllPath))
                    {
                        if (pathIndex == paths.Length - 1)
                        {
                            Debug.LogError("Failed to load genvid.dll from " + Application.dataPath + dllPath);
                            return;
                        }
                        else
                        {
                            ++pathIndex;
                            continue;
                        }
                    }
                    else
                    {
                        Debug.Log("genvid.dll successfully loaded from " + Application.dataPath + dllPath);
                        break;
                    }
                }

                GenvidSDK.Status gvStatus = GenvidSDK.Initialize();
                if (GenvidSDK.StatusFailed(gvStatus))
                {
                    m_State = State.Uninitialized;
                    Debug.LogError("Error running Genvid Initialize : " + GenvidSDK.StatusToString(gvStatus));
                }
                else
                {
                    m_State = State.Initialized;
                    if (ActivateDebugLog)
                    {
                        Debug.Log("Genvid Initialize performed correctly.");
                    }
                }

                //In case of User manually doing initialize
                if (!m_IsCreated)
                {
                    OnEnable();
                }
            }
#endif
        }

        /// <summary>
        /// Terminates the session and SDK.
        /// </summary>
        public void Terminate()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // SafeApplicationQuit() sets m_State to Destroying.
            if (ActivateSDK && (IsInitialized || IsDestroying))
            {
                var gvStatus = GenvidSDK.Terminate();
                if (GenvidSDK.StatusFailed(gvStatus))
                {
                    Debug.LogError("Error while doing the terminate process : " + GenvidSDK.StatusToString(gvStatus));
                }
                else
                {
                    m_State = State.Uninitialized;
                    if (ActivateDebugLog)
                    {
                        Debug.Log("Genvid Terminate performed correctly.");
                    }
                }

                GenvidSDK.UnloadGenvidDll();
            }
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        /// <summary>
        /// Terminates the session and SDK. Used as a coroutine.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SafeApplicationQuit()
        {
            m_State = State.Destroying;

            OnDisable();

            if (ActivateSDK && Session != null)
            {
                DestroyImmediate(Session);
            }

            Terminate();
            yield return null;
        }

        /// <summary>
        /// Initialized the SDK and session on monobehaviour activation.
        /// </summary>
        private void Awake()
        {
            m_State = State.Initializing;

            /// Need to keep the instance alive before switching scene.
            /// Mandatory when Genvid SDK is not active.
            var instanceInit = Instance;

            if (AutoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Evaluates if new events have been received. Checks once per frame.
        /// </summary>
        private void Update()
        {
            if (ActivateSDK && m_IsCreated && IsInitialized)
            {
                var gvStatus = GenvidSDK.CheckForEvents();
                if ((GenvidSDK.StatusFailed(gvStatus)) && gvStatus != GenvidSDK.Status.ConnectionTimeout)
                {
                    Debug.LogError("Error while running CheckForEvents : " + GenvidSDK.StatusToString(gvStatus));
                }
                else if (ActivateDebugLog)
                {
                    Debug.Log("Genvid CheckForEvents performed correctly.");
                }
            }
        }

        /// <summary>
        /// Creates the session when the monobehaviour is enabled.
        /// </summary>
        private void OnEnable()
        {
            if (Session != null && !m_IsCreated && ActivateSDK && IsInitialized)
            {
                Session.Create();
                m_IsCreated = true;
            }
        }

        /// <summary>
        /// Destroys the session when the monobehaviour is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (Session != null && m_IsCreated && ActivateSDK && (IsInitialized || IsDestroying))
            {
                Session.Destroy();
                m_IsCreated = false;
            }
        }

        /// <summary>
        /// Starts a coroutine to safely terminate the SDK and session.
        /// </summary>
        private void OnApplicationQuit()
        {
            if (ActivateSDK && IsInitialized)
            {
                StartCoroutine(SafeApplicationQuit());
                m_State = State.Destroyed;
            }
        }
#endif
    }
}