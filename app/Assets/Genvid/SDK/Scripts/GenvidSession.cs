using GenvidSDKCSharp;
using UnityEngine;
using System;
using System.Text;
using Genvid;

namespace Genvid
{
    namespace Plugin
    {
        /// <summary>
        /// The session manages all components necessary for a complete
        /// streaming experience: audio, video, data streams, commands, and events.
        /// </summary>
        [Serializable]
        public class Session : IGenvidPlugin
        {
            /// <summary>
            /// The video component.
            /// </summary>
            [SerializeField]
            private Stream.Video m_Video;

            /// <summary>
            /// The audio component.
            /// </summary>
            [SerializeField]
            private Stream.Audio m_Audio;

            /// <summary>
            /// The data-streams component.
            /// </summary>
            [SerializeField]
            private Stream.Data m_Streams;

            /// <summary>
            /// The events component.
            /// </summary>
            [SerializeField]
            private Channel.Events m_Events;

            /// <summary>
            /// The commands component.
            /// </summary>
            [SerializeField]
            private Channel.Commands m_Commands;

            /// <summary>
            /// Keeps track of the component state.
            /// Toggled on when initalization succeeds.
            /// Toggled off when the plugin is disabled.
            /// </summary> 
            public bool IsInitialized { get; private set; }

            /// <summary>
            /// Specifies logging verbosity. Toggle on for more logging info.
            /// </summary>
            public bool VerboseLog { get; set; }

            /// <summary>
            /// The video component getter/setter.
            /// </summary>
            public Stream.Video Video { get { return m_Video; } set { m_Video = value; } }

            /// <summary>
            /// The audio component getter/setter.
            /// </summary>
            public Stream.Audio Audio { get { return m_Audio; } set { m_Audio = value; } }

            /// <summary>
            /// The data streams component getter/setter.
            /// </summary>
            public Stream.Data Data { get { return m_Streams; } set { m_Streams = value; } }

            /// <summary>
            /// The events component getter/setter.
            /// </summary>
            public Channel.Events Events { get { return m_Events; } set { m_Events = value; } }

            /// <summary>
            /// The commands component getter/setter.
            /// </summary>
            public Channel.Commands Commands { get { return m_Commands; } set { m_Commands = value; } }

            /// <summary>
            /// Initializes all components.
            /// </summary>
            /// <returns>True if all components initialize correctly, false otherwise.</returns>
            public bool Initialize()
            {
                bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (IsInitialized)
                {
                    return true;
                }

                if (Video.Initialize() == false)
                {
                    result = false;
                    Debug.LogError("GenvidSession failed to initialize the video stream!");
                }

                if (Audio.Initialize() == false)
                {
                    result = false;
                    Debug.LogError("GenvidSession failed to initialize an audio stream!");
                }

                if (Data.Initialize() == false)
                {
                    result = false;
                    Debug.LogError("GenvidSession failed to initialize gamedata streams!");
                }

                if (Events.Initialize() == false)
                {
                    result = false;
                    Debug.LogError("GenvidSession failed to initialize Genvid events!");
                }

                if (Commands.Initialize() == false)
                {
                    result = false;
                    Debug.LogError("GenvidSession failed to initialize Genvid commands!");
                }

#endif
                IsInitialized = result;
                return result;
            }

            /// <summary>
            /// Cleans up all components.
            /// </summary>
            /// <returns>True if all components terminate correctly, false otherwise.</returns>
            public bool Terminate()
            {
                bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (IsInitialized == false)
                {
                    return true;
                }

                if (Commands != null)
                {
                    if (Commands.Terminate() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to terminate Genvid commands!");
                    }
                }

                if (Events != null)
                {
                    if (Events.Terminate() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to terminate Genvid events!");
                    }
                }

                if (Data != null)
                {
                    if (Data.Terminate() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to terminate gamedata streams!");
                    }
                }

                if (Audio != null)
                {
                    if (Audio.Terminate() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to terminate an audio stream!");
                    }
                }

                if (Video != null)
                {
                    if (Video.Terminate() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to terminate the video stream!");
                    }
                }
#endif
                IsInitialized = !result;
                return result;
            }

            /// <summary>
            /// Starts all components.
            /// </summary>
            public void Start()
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (Video.IsInitialized)
                {
                    Video.Start();
                }
                if (Audio.IsInitialized)
                {
                    Audio.Start();
                }
                if (Data.IsInitialized)
                {
                    Data.Start();
                }
                if (Events.IsInitialized)
                {
                    Events.Start();
                }
                if (Commands.IsInitialized)
                {
                    Commands.Start();
                }
#endif
            }

            /// <summary>
            /// Updates all components. Called once per frame.
            /// </summary>
            public void Update()
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (Video.IsInitialized)
                {
                    Video.Update();
                }
                if (Audio.IsInitialized)
                {
                    Audio.Update();
                }
                if (Data.IsInitialized)
                {
                    Data.Update();
                }
                if (Events.IsInitialized)
                {
                    Events.Update();
                }
                if (Commands.IsInitialized)
                {
                    Commands.Update();
                }
#endif
            }

            /// <summary>
            /// Submits a notification.
            /// </summary>
            /// <param name="notificationID">ID of the notification.</param>
            /// <param name="data">Data to submit.</param>
            /// <param name="size">Size of data to submit</param>
            /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
            public bool SubmitNotification(object notificationID, byte[] data, int size)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (!IsInitialized)
                {
                    Debug.LogError("Genvid Session is not initialized: Unable to submit notification.");
                    return false;
                }

                var status = GenvidSDK.SubmitNotification(notificationID.ToString(), data, size);

                if (GenvidSDK.StatusFailed(status))
                {
                    Debug.LogError(String.Format("`SubmitNotification` failed with error: {0}.", GenvidSDK.StatusToString(status)));
                    return false;
                }

                if (VerboseLog)
                {
                    Debug.Log(String.Format("Genvid correctly submitted notification: {0}.", data));
                }
#endif
                return true;
            }

            /// <summary>
            /// Submits a notification.
            /// </summary>
            /// <param name="notificationID">ID of the notification.</param>
            /// <param name="data">Data to submit.</param>
            /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
            public bool SubmitNotification(object notificationID, byte[] data)
            {
                return SubmitNotification(notificationID, data, data.Length);
            }

            /// <summary>
            /// Submits a notification.
            /// </summary>
            /// <param name="notificationID">ID of the notification.</param>
            /// <param name="data">Data to submit.</param>
            /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
            public bool SubmitNotification(object notificationID, string data)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (data == null)
                {
                    Debug.LogError("Unable to handle `null` data. Submitting notification failed.");
                    return false;
                }

                var dataAsBytes = Encoding.Default.GetBytes(data);
                return SubmitNotification(notificationID, dataAsBytes);
#else
                return true;
#endif
            }

            /// <summary>
            /// Submits a notification.
            /// Notification data object is serialized to JSON before submission.
            /// </summary>
            /// <param name="notificationID">ID of the notification.</param>
            /// <param name="data">Data object to submit</param>
            /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
            public bool SubmitNotificationJSON(object notificationID, object data)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (data == null)
                {
                    Debug.LogError("Unable to handle `null` data. Submitting notification failed.");
                    return false;
                }

                var jsonData = GenvidPlugin.SerializeToJSON(data);

                if (jsonData == null)
                {
                    Debug.LogError(String.Format("Failed to send notification with ID '{0}' due to a JSON serialization error.", notificationID));
                    return false;
                }

                return SubmitNotification(notificationID, jsonData);
#else
                return true;
#endif
            }
        }
    }

    /// <summary>
    /// The session manages all components necessary for a complete
    /// streaming experience: audio, video, data streams, commands, and events.
    /// </summary>
    [System.Obsolete("Deprecated. Call `Genvid.Plugin.Session` to use Genvid session.")]
    public class GenvidSession : MonoBehaviour, IGenvidBase
    {
        /// <summary>
        /// The video component.
        /// </summary>
        public GenvidVideo VideoStream;

        /// <summary>
        /// The audio component.
        /// </summary>
        public GenvidAudio AudioStream;

        /// <summary>
        /// The data-streams component.
        /// </summary>
        public GenvidStreams Streams;

        /// <summary>
        /// The events component.
        /// </summary>
        public GenvidEvents Events;

        /// <summary>
        /// The commands component.
        /// </summary>
        public GenvidCommands Commands;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
    // Disable warning for other platforms.
#pragma warning disable 414
#endif

        /// <summary>
        /// Keeps track of the component state.
        /// Toggled on when initalization succeeds.
        /// Toggled off when the plugin is disabled.
        /// </summary> 
        private bool m_IsCreated = false;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
#pragma warning restore 414
#endif

        /// <summary>
        /// Initializes all components.
        /// </summary>
        /// <returns>True if all components initialized correctly, false otherwise.</returns>
        public bool Create()
        {
            bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK && !m_IsCreated && GenvidSessionManager.IsInitialized)
            {
                if (VideoStream != null)
                {
                    if (VideoStream.Create() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to create a video stream!");
                    }
                }
                else
                {
                    Debug.LogWarning("GenvidSession doesn't have a GenvidVideo GameObject linked to it!");
                }

                if (AudioStream != null)
                {
                    if (AudioStream.Create() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to create an audio stream!");
                    }
                }
                else
                {
                    Debug.LogWarning("GenvidSession doesn't have a GenvidAudio GameObject linked to it!");
                }

                if (Streams != null)
                {
                    if (Streams.Create() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to create gamedata streams!");
                    }
                }
                else
                {
                    Debug.LogWarning("GenvidSession doesn't have a GenvidStreams GameObject linked to it!");
                }

                if (Events != null)
                {
                    if (Events.Create() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to create Genvid events!");
                    }
                }

                if (Commands != null)
                {
                    if (Commands.Create() == false)
                    {
                        result = false;
                        Debug.LogError("GenvidSession failed to create Genvid commands!");
                    }
                }

                m_IsCreated = true;
            }
#endif
            return result;
        }

        /// <summary>
        /// Cleans up all components.
        /// </summary>
        /// <returns>True if all components terminated correctly, false otherwise.</returns>
        public bool Destroy()
        {
            bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK && m_IsCreated)
            {
                if (Commands) result &= Commands.Destroy();
                if (Events) result &= Events.Destroy();
                if (Streams) result &= Streams.Destroy();
                if (AudioStream) result &= AudioStream.Destroy();
                if (VideoStream) result &= VideoStream.Destroy();
                m_IsCreated = false;
            }
#endif
            return result;
        }

        /// <summary>
        /// Submits a notification.
        /// </summary>
        /// <param name="notificationID">ID of the notification.</param>
        /// <param name="data">Data to submit.</param>
        /// <param name="size">Size of data to submit</param>
        /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
        public bool SubmitNotification(object notificationID, byte[] data, int size)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (!m_IsCreated)
            {
                if (GenvidSessionManager.Instance.ActivateSDK && !GenvidSessionManager.IsInitialized)
                {
                    Debug.LogError("Genvid SDK is not initialized: Unable to submit notification.");
                }
                else
                {
                    Debug.LogError(String.Format("Unable to submit notification with ID '{0}'.", notificationID));
                }

                return false;
            }

            var status = GenvidSDK.SubmitNotification(notificationID.ToString(), data, size);

            if (GenvidSDK.StatusFailed(status))
            {
                Debug.LogError(String.Format("`SubmitNotitication` failed with error: {0}.", GenvidSDK.StatusToString(status)));
                return false;
            }

            if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log(String.Format("Genvid correctly submitted notification: {0}.", data));
            }
#endif
            return true;
        }

        /// <summary>
        /// Submits a notification.
        /// </summary>
        /// <param name="notificationID">ID of the notification.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
        public bool SubmitNotification(object notificationID, byte[] data)
        {
            return SubmitNotification(notificationID, data, data.Length);
        }

        /// <summary>
        /// Submits a notification.
        /// </summary>
        /// <param name="notificationID">ID of the notification.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
        public bool SubmitNotification(object notificationID, string data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (data == null)
            {
                Debug.LogError("Unable to handle `null` data. Submitting notification failed.");
                return false;
            }

            var dataAsBytes = Encoding.Default.GetBytes(data);
            return SubmitNotification(notificationID, dataAsBytes);
#else
        return true;
#endif
        }

        /// <summary>
        /// Submits a notification.
        /// Notification data object is serialized to JSON before submission.
        /// </summary>
        /// <param name="notificationID">ID of the notification.</param>
        /// <param name="data">Data object to submit</param>
        /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
        public bool SubmitNotificationJSON(object notificationID, object data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (data == null)
            {
                Debug.LogError("Unable to handle `null` data. Submitting notification failed.");
                return false;
            }

            var jsonData = Genvid.Plugin.GenvidPlugin.SerializeToJSON(data);

            if (jsonData == null)
            {
                Debug.LogError(String.Format("Failed to send notification with ID '{0}' due to a JSON serialization error.", notificationID));
                return false;
            }

            return SubmitNotification(notificationID, jsonData);
#else
       return true;
#endif
        }

        /// <summary>
        /// Submits a notification.
        /// Notification data object is serialized to JSON before submission.
        /// </summary>
        /// <param name="notificationID">ID of the notification.</param>
        /// <param name="data">Data object to submit.</param>
        /// <returns>True if the notification was successfully submitted, false otherwise.</returns>
        [System.Obsolete("This is an obsolete overload. Use `SubmitNotificationJSON` if you're submitting JSON serializable data.")]
        public bool SubmitNotification(object notificationID, object data)
        {
            return SubmitNotificationJSON(notificationID, data);
        }
    }
}