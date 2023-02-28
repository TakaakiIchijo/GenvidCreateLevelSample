using UnityEngine;
using System;
using GenvidSDKCSharp;
using Genvid;

namespace Genvid
{
    namespace Plugin
    {
        namespace Stream
        {
            /// <summary>
            /// Helper class that handles initializing and terminating the audio stream.
            /// </summary>
            [Serializable]
            public class Audio : IGenvidPlugin
            {
                /// <summary>
                /// Audio parameters specified by the user.
                /// </summary>
                public GenvidAudioParameters Settings;

                /// <summary>
                /// True if the audio stream has been initialized, false otherwise. 
                /// </summary>
                public bool IsInitialized { get; private set; }

                /// <summary>
                /// Specifies logging verbosity. Toggle on for more logging info.
                /// </summary>
                public bool VerboseLog { get; set; }

                /// <summary>
                /// The sampling rate.
                /// </summary>
                public int AudioRate { get; private set; }

                /// <summary>
                /// The number of audio channels.
                /// </summary>
                public int AudioChannels { get; private set; }

                /// <summary>
                /// The current audio listener.
                /// </summary>
                private AudioListener m_AudioListener;

                /// <summary>
                /// The stream filter used with the listener.
                /// </summary>
                private AudioStreamFilter m_AudioStreamFilter;

                /// <summary>
                /// Intializes the audio stream and listeners depending on the selected audio mode.
                /// </summary>
                /// <returns>True if stream was created successfully, false otherwise.</returns>
                public bool Initialize()
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (!IsInitialized && Settings != null)
                    {
                        GenvidStreamUtils.VerboseLog = VerboseLog;
                        if (GenvidStreamUtils.CreateStream(Settings.Id))
                        {
                            if (Settings.AudioMode == GenvidAudioParameters.AudioCapture.None)
                            {
                                Debug.LogWarning("No audio mode set. There will be no audio capture!");
                            }
                            else if (Settings.AudioMode == GenvidAudioParameters.AudioCapture.WASAPI)
                            {
                                Debug.Log("Audio mode set to Wasapi.");
                                GenvidAudioUtils.SetAudioWasapiMode(Settings.Id, true);
                            }
                            else if (Settings.AudioMode == GenvidAudioParameters.AudioCapture.Unity)
                            {
                                Debug.Log("Audio mode set to Unity.");

                                GenvidAudioUtils.SetupUnityAudioListeners(GenvidPlugin.Instance.gameObject, Settings.Listener, out m_AudioListener, out m_AudioStreamFilter);

                                int audiorate;
                                int channels;
                                if (GenvidAudioUtils.SetupGenvidUnityAudio(Settings.Id, Settings.AudioFormat, out audiorate, out channels))
                                {
                                    AudioRate = audiorate;
                                    AudioChannels = channels;
                                }

                                m_AudioStreamFilter.OnAudioReceivedDataCallback += OnAudioReceivedDataCallback;
                            }

                            IsInitialized = true;
                        }
                    }

                    return IsInitialized;
#else
                return true;
#endif
                }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                /// <summary>
                /// Submits audio data to the SDK.
                /// </summary>
                /// <param name="data">The data to submit.</param>
                /// <param name="otherwise">The number of channels.</param>
                private void OnAudioReceivedDataCallback(float[] data, int channels)
                {
                    if (Settings.AudioFormat == GenvidSDK.AudioFormat.S16LE)
                    {
                        GenvidAudioUtils.SubmitAudioData<short[]>(Settings.Id, data, channels);
                    }
                    else if (Settings.AudioFormat == GenvidSDK.AudioFormat.F32LE)
                    {
                        GenvidAudioUtils.SubmitAudioData<float[]>(Settings.Id, data, channels);
                    }
                }
#endif
                /// <summary>
                /// Destroys the audio stream and listeners.
                /// </summary>
                /// <returns>True if stream was successfully destroyed, false otherwise.</returns>
                public bool Terminate()
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (IsInitialized)
                    {
                        if (Settings.AudioMode == GenvidAudioParameters.AudioCapture.Unity)
                        {
                            m_AudioStreamFilter.OnAudioReceivedDataCallback -= OnAudioReceivedDataCallback;
                            UnityEngine.Object.Destroy(m_AudioStreamFilter);
                            UnityEngine.Object.Destroy(m_AudioListener);
                        }

                        if (GenvidStreamUtils.DestroyStream(Settings.Id))
                        {
                            IsInitialized = false;
                        }
                    }

                    return !IsInitialized;
#else
                return true;
#endif
                }

                /// <summary>
                /// Empty override of the IGenvidPlugin interface.
                /// All setup is done during the initialization phase.
                /// </summary>
                public void Start()
                {
                    /* Nothing to do*/
                }

                /// <summary>
                /// Empty override of the IGenvidPlugin interface.
                /// No need to update as the sampling is done automatically.
                /// </summary>
                public void Update()
                {
                    /* Nothing to do*/
                }
            }
        }
    }

    /// <summary>
    /// Helper class that handles initializing and terminating the audio stream.
    /// </summary>
    [System.Obsolete("Please consider using `Genvid.Plugin.GenvidAudio` to use genvid audio stream.")]
    public class GenvidAudio : GenvidStreamBase
    {
        /// <summary>
        /// The audio mode selection.
        /// </summary>
        public enum AudioMode
        {
            None,
            WASAPI,
            Unity
        }

        // Disable warning for other platforms.
#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
#pragma warning disable 414
#endif

        /// <summary>
        /// The stream name.
        /// </summary>
        [SerializeField]
        private string m_StreamName;

        /// <summary>
        /// The audio format.
        /// </summary>
        [SerializeField]
        private GenvidSDK.AudioFormat m_AudioFormat = GenvidSDK.AudioFormat.S16LE;

        /// <summary>
        /// The audio mode.
        /// </summary>
        [SerializeField]
        private AudioMode m_AudioMode = AudioMode.Unity;

        /// <summary>
        /// The audio listener.
        /// </summary>
        [SerializeField]
        private AudioListener m_AudioListener;

        /// <summary>
        /// The audio stream filter.
        /// </summary>
        private AudioStreamFilter m_AudioStreamFilter;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
#pragma warning restore 414
#endif

        /// <summary>
        /// The audio stream name getter/setter.
        /// </summary>
        public string StreamName
        {
            get { return m_StreamName; }
            private set { m_StreamName = value; }
        }

        /// <summary>
        /// The audio format getter/setter.
        /// </summary>
        public GenvidSDK.AudioFormat AudioFormat
        {
            get
            {
                return Genvid.Plugin.GenvidAudioUtils.GetAudioFormat(m_StreamName);
            }
            set
            {
                Genvid.Plugin.GenvidAudioUtils.SetAudioFormat(m_StreamName, value);
            }
        }

        /// <summary>
        /// The number of channels getter/setter.
        /// </summary>
        public int AudioChannels
        {
            get
            {
                return Genvid.Plugin.GenvidAudioUtils.GetAudioChannels(m_StreamName);
            }
            set
            {
                Genvid.Plugin.GenvidAudioUtils.SetAudioChannels(m_StreamName, value);
            }
        }

        /// <summary>
        /// The audio rate getter/setter.
        /// </summary>
        public int AudioRate
        {
            get
            {
                return Genvid.Plugin.GenvidAudioUtils.GetAudioRate(m_StreamName);
            }
            set
            {
                GenvidSDK.SetParameter(m_StreamName, "audio.rate", value);
            }
        }

        /// <summary>
        /// The audio granularity getter/setter.
        /// </summary>
        public int AudioGranularity
        {
            get
            {
                return Genvid.Plugin.GenvidAudioUtils.GetAudioGranularity(m_StreamName);
            }
            set
            {
                Genvid.Plugin.GenvidAudioUtils.SetAudioGranularity(m_StreamName, value);
            }
        }

        /// <summary>
        /// Log verbosity initialization.
        /// </summary>
        private void Awake()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Genvid.Plugin.GenvidAudioUtils.Verbose = GenvidSessionManager.Instance.ActivateDebugLog;
#endif
        }

        /// <summary>
        /// Intializes the audio stream and listeners depending on the selected audio mode.
        /// </summary>
        /// <returns>True if the stream was created successfully, false otherwise.</returns>
        public override bool Create()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK && GenvidSessionManager.IsInitialized)
            {
                ParseCommandLine();

                var status = GenvidSDK.CreateStream(m_StreamName);
                if (GenvidSDK.StatusFailed(status))
                {
                    Debug.LogError("Error while creating the " + m_StreamName + " stream: " + GenvidSDK.StatusToString(status));
                    return false;
                }
                else if (GenvidSessionManager.Instance.ActivateDebugLog)
                {
                    Debug.Log("Genvid create audio stream named " + m_StreamName + " performed correctly.");
                }

                if (m_AudioMode != AudioMode.None)
                {
                    if (GenvidSessionManager.Instance.Session.VideoStream == null)
                    {
                        Debug.LogError("Error while Accessing Video framerate from " + m_StreamName + " stream.");
                        return false;
                    }
                }

                if (m_AudioMode == AudioMode.WASAPI)
                {
                    Genvid.Plugin.GenvidAudioUtils.SetAudioWasapiMode(m_StreamName, true);
                }
                else if (m_AudioMode == AudioMode.Unity)
                {
                    Genvid.Plugin.GenvidAudioUtils.SetupUnityAudioListeners(gameObject, m_AudioListener, out m_AudioListener, out m_AudioStreamFilter);

                    int audiorate;
                    int channels;
                    if (Genvid.Plugin.GenvidAudioUtils.SetupGenvidUnityAudio(m_StreamName, m_AudioFormat, out audiorate, out channels))
                    {
                        m_AudioStreamFilter.OnAudioReceivedDataCallback += OnAudioReceivedDataCallback;
                    }
                    else
                    {
                        Debug.LogError("Failed to setting up audio stream.");
                    }
                }
            }
#endif
            return true;
        }

        /// <summary>
        /// Destroys the audio stream and listeners.
        /// </summary>
        /// <returns>True if the stream was successfully destroyed, false otherwise.</returns>
        public override bool Destroy()
        {
            bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK)
            {
                if (m_AudioStreamFilter != null)
                {
                    m_AudioStreamFilter.OnAudioReceivedDataCallback -= OnAudioReceivedDataCallback;
                    UnityEngine.Object.Destroy(m_AudioStreamFilter);
                    UnityEngine.Object.Destroy(m_AudioListener);
                }

                var status = GenvidSDK.DestroyStream(m_StreamName);
                if (GenvidSDK.StatusFailed(status))
                {
                    result = false;
                    Debug.LogError("Error while destroying the " + m_StreamName + " stream: " + GenvidSDK.StatusToString(status));
                }
                else if (GenvidSessionManager.Instance.ActivateDebugLog)
                {
                    Debug.Log("Genvid Destroy audio stream named " + m_StreamName + " performed correctly.");
                }
            }
#endif
            return result;
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        /// <summary>
        /// Submits audio data to the SDK.
        /// </summary>
        /// <param name="data">The data to submit.</param>
        /// <param name="otherwise">The number of channels.</param>
        private void OnAudioReceivedDataCallback(float[] data, int channels)
        {
            if (m_AudioFormat == GenvidSDK.AudioFormat.S16LE)
            {
                Genvid.Plugin.GenvidAudioUtils.SubmitAudioData<short[]>(m_StreamName, data, channels);
            }
            else if (m_AudioFormat == GenvidSDK.AudioFormat.F32LE)
            {
                Genvid.Plugin.GenvidAudioUtils.SubmitAudioData<float[]>(m_StreamName, data, channels);
            }
        }

        /// <summary>
        /// Check if the evaluated value is part of an enumerator's values.
        /// </summary>
        /// <param name="enumValue">The value to evaluate.</param>
        /// <returns>True if the value is part of the enum, false otherwise.</returns>
        private bool IsEnumDefined<T>(string enumValue)
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                if (String.Equals(value.ToString(), enumValue, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Set the audio mode according to the command line input.
        /// </summary>
        /// <param name="argValue">The audio mode as a string.</param>
        private void ParseCommandLine()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-AudioMode")
                {
                    bool isDefined = false;

                    if ((i + 1) < args.Length)
                    {
                        var argValue = args[i + 1];
                        isDefined = IsEnumDefined<AudioMode>(argValue);

                        if (isDefined)
                        {
                            m_AudioMode = (AudioMode)Enum.Parse(typeof(AudioMode), argValue, true);
                            Debug.Log("Forcing audio mode to '" + argValue + "'");
                        }
                        else
                        {
                            Debug.LogError("Failed to parse AudioMode, '" + argValue + "' is unknown.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to find a value for the AudioMode parameter.");
                    }

                    if (!isDefined)
                    {
                        Debug.LogError("Use one of this AudioMode: " + String.Join(", ", Enum.GetNames(typeof(AudioMode))) + ".");
                    }
                    break;
                }
            }
        }
#endif
    }
}