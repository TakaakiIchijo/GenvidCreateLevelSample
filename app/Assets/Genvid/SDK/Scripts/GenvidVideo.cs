using UnityEngine;
using System.Collections;
using System;
using Genvid;
using Genvid.Plugin;

namespace Genvid
{
    namespace Plugin
    {
        namespace Stream
        {
            /// <summary>
            /// Helper class that handles state changes and data submissions for video streams.
            /// </summary>
            [Serializable]
            public class Video : IGenvidPlugin
            {
                /// <summary>
                /// Used to keep the video submitter from being destroyed between scenes.
                /// </summary>
                private class DummyMonoBehavior : MonoBehaviour { }

                /// <summary>
                /// Toggled on when stream initialization is done.
                /// </summary> 
                public bool IsInitialized { get; private set; }

                /// <summary>
                /// Specifies logging verbosity. Toggle on for more logging info.
                /// </summary>
                public bool VerboseLog { get; set; }

                /// <summary>
                /// The user-defined parameters for this video stream.
                /// </summary>
                [Tooltip("Video Settings")]
                public GenvidVideoParameters Settings;

                /// <summary>
                /// The video capture source. Used when the SDK automatic-capture mode isn't selected.
                /// </summary>
                [SerializeField]
                [Tooltip("Video Source (Texture or Camera)")]
                private UnityEngine.Object m_Source;

                /// <summary>
                /// Returns the video-capture source.
                /// </summary>
                public UnityEngine.Object Source
                {
                    get { return m_Source; }
                    set
                    {
                        m_Source = value;
                        if (m_GenvidVideoCapture != null)
                        {
                            m_GenvidVideoCapture.VideoSource = value;
                        }
                    }
                }

                /// <summary>
                /// Helper to facilitate video capture.
                /// </summary>
                private GenvidVideoCapture m_GenvidVideoCapture;

                /// <summary>
                /// Dummy object used to keep the video submitter from being destroyed between scenes.
                /// </summary>
                private DummyMonoBehavior m_DummyMonoBehavior;

                /// <summary>
                /// The periodic deadline keeps track of when to invoke
                /// the stream-submission callback based on the stream framerates.
                /// </summary>
                private PeriodicDeadline m_Deadline = new PeriodicDeadline();

                /// <summary>
                /// Returns the periodic deadline object.
                /// </summary>
                private PeriodicDeadline Deadline
                {
                    get
                    {
                        // Ensure framerate is consistent.
                        m_Deadline.Framerate = Settings.Framerate;
                        return m_Deadline;
                    }
                }

                /// <summary>
                /// Initializes a new video stream with the user-defined parameters.
                /// Starts the capture coroutine.
                /// </summary>
                /// <returns>True if the stream is successfully created, false otherwise.</returns>
                public bool Initialize()
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (!IsInitialized)
                    {
                        GenvidVideoUtils.VerboseLog = VerboseLog;
                        bool succeeded = GenvidVideoUtils.CreateStream(Settings.Id, Settings.Framerate);
                        if (succeeded)
                        {
                            /// Starts video capture.
                            m_GenvidVideoCapture = new GenvidVideoCapture(Settings.Id, Settings.Framerate, Settings.CaptureType, Source);
                            m_GenvidVideoCapture.VerboseLog = VerboseLog;

                            GameObject singleton = new GameObject("Genvid Video Capture (Singleton)");
                            m_DummyMonoBehavior = singleton.AddComponent<DummyMonoBehavior>();
                            UnityEngine.Object.DontDestroyOnLoad(singleton);
                            m_DummyMonoBehavior.StartCoroutine(m_GenvidVideoCapture.CallPluginAtEndOfFrames(m_Deadline, GenvidPlugin.Instance.Settings.DisableVideoDataSubmissionThrottling));
                            IsInitialized = true;
                        }
                    }

                    return IsInitialized;
#else
                return true;
#endif
                }

                /// <summary>
                /// Destroys the video stream and ends the capture process.
                /// </summary>
                /// <returns>True if the stream is successfully destroyed, false otherwise.</returns>
                public bool Terminate()
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (IsInitialized)
                    {
                        if (GenvidVideoUtils.DestroyStream(Settings.Id))
                        {
                            m_GenvidVideoCapture.QuitProcess();
                            m_GenvidVideoCapture.CleanupResources();
                            UnityEngine.Object.Destroy(m_DummyMonoBehavior);
                            IsInitialized = false;
                        }
                        else
                        {
                            return false;
                        }
                    }
#endif
                    return true;
                }

                /// <summary>
                /// Nothing to do for the video on session start.
                /// </summary>
                public void Start()
                {
                    /* Nothing to do*/
                }

                /// <summary>
                /// Nothing to do for the video on session update.
                /// The capture coroutine takes care of everything. Updates occur on end of frame.
                /// </summary>
                public void Update()
                {
                    /* Nothing to do*/
                }
            }
        }
    }

    [System.Obsolete("Deprecated. Call `Genvid.Plugin.GenvidVideo` to use Genvid video stream.")]
    public class GenvidVideo : GenvidStreamBase
    {
        /// <summary>
        /// The video stream name.
        /// </summary>
        [SerializeField]
        [Tooltip("Video Stream Name")]
        private string m_StreamName;

        /// <summary>
        /// The video stream framerate.
        /// </summary>
        [SerializeField]
        [Range(30.0f, 60.0f)]
        [Tooltip("Video Stream Framerate")]
        private float m_Framerate = 30.0f;

        /// <summary>
        /// The video stream capture type: texture or automatic.
        /// </summary>
        [SerializeField]
        [Tooltip("Video Capture Type")]
        private GenvidVideoParameters.eCaptureType m_CaptureType;

        /// <summary>
        /// The video capture source. Use when not in automatic mode.
        /// </summary>
        [SerializeField]
        [Tooltip("Video Source (Texture or Camera)")]
        private UnityEngine.Object m_VideoSource;

        /// <summary>
        /// Helper to facilitate video capture.
        /// </summary>
        private GenvidVideoCapture m_GenvidVideoCapture;

        /// <summary>
        /// The periodic deadline keeps track of when to invoke
        /// the stream-submission callback based on the stream framerates.
        /// </summary>
        private PeriodicDeadline m_Deadline = new PeriodicDeadline();

        /// <summary>
        /// Returns the periodic deadline object.
        /// </summary>
        private PeriodicDeadline Deadline
        {
            get
            {
                /// Ensure the framerate is consistent.
                m_Deadline.Framerate = Framerate;
                return m_Deadline;
            }
        }

        /// <summary>
        /// The video stream name getter/setter.
        /// </summary>
        public string StreamName
        {
            get { return m_StreamName; }
            private set { m_StreamName = value; }
        }

        /// <summary>
        /// The video stream framerate getter/setter.
        /// </summary>
        public float Framerate
        {
            get { return m_Framerate; }
            private set
            {
                m_Framerate = value;
                Deadline.Framerate = value;
            }
        }

        /// <summary>
        /// The video stream capture-type getter/setter.
        /// </summary>
        public GenvidVideoParameters.eCaptureType CaptureType
        {
            get { return m_CaptureType; }
            set { m_CaptureType = value; }
        }

        /// <summary>
        /// The video-capture source capture-type getter/setter.
        /// </summary>
        public UnityEngine.Object VideoSource
        {
            get { return m_VideoSource; }
            set
            {
                m_VideoSource = value;
                if (m_GenvidVideoCapture != null)
                {
                    m_GenvidVideoCapture.VideoSource = value;
                }
            }
        }

        /// <summary>
        /// Toggled on when stream initialization is done.
        /// </summary> 
        public bool IsCreated { get; private set; }

        /// GENVID - On start begin

        /// <summary>
        /// Initializes a new video stream with the user-defined parameters.
        /// Starts the capture coroutine.
        /// </summary>
        /// <returns>True if the stream is successfully created, false otherwise.</returns>
        public override bool Create()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK && !IsCreated && GenvidSessionManager.IsInitialized)
            {
                GenvidVideoUtils.VerboseLog = GenvidSessionManager.Instance.ActivateDebugLog;
                bool succeeded = GenvidVideoUtils.CreateStream(StreamName, Framerate);
                if (succeeded)
                {
                    /// Starts video capture.
                    m_GenvidVideoCapture = new GenvidVideoCapture(StreamName, Framerate, CaptureType, VideoSource);
                    m_GenvidVideoCapture.VerboseLog = GenvidSessionManager.Instance.ActivateDebugLog;
                    StartCoroutine(m_GenvidVideoCapture.CallPluginAtEndOfFrames(Deadline, GenvidSessionManager.DisableVideoDataSubmissionThrottling));
                    IsCreated = true;
                }
                else
                {
                    IsCreated = false;
                }
            }
#endif
            return true;
        }
        /// GENVID - On start end

        /// <summary>
        /// Destroys the video stream and ends the capture process.
        /// </summary>
        /// <returns>True if the stream is successfully destroyed, false otherwise.</returns>
        public override bool Destroy()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (IsCreated)
            {
                if (GenvidVideoUtils.DestroyStream(StreamName))
                {
                    m_GenvidVideoCapture.QuitProcess();
                    StartCoroutine(DestroyVideo());
                }
            }
#endif
            return true;
        }

        /// <summary>
        /// Waits for the video capture coroutine to exit if the session manager isn't
        /// currently terminating the session. Then it cleans up video capture resources.
        /// </summary>
        /// <returns>Returns a "wait until" object for the end of the video capture coroutine.</returns>
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private IEnumerator DestroyVideo()
        {
            yield return m_GenvidVideoCapture.DestroyVideo(GenvidSessionManager.IsDestroying);
            IsCreated = false;
        }
#endif
    }
}