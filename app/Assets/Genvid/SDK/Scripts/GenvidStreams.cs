using GenvidSDKCSharp;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using System.Collections.Generic;
using Genvid;

namespace Genvid
{
    namespace Plugin
    {
        namespace Stream
        {
            /// <summary>
            /// Helper class that handles state changes and data submissions for data streams and annotations.
            /// </summary>
            [Serializable]
            public class Data : IGenvidPlugin
            {
                /// <summary>
                /// Class representing a stream event as a Unity event.
                /// </summary>
                [Serializable]
                public class StreamEvent : UnityEvent<string>
                {
                }

                /// <summary>
                /// List of data-stream parameters.
                /// Streams are created from this list.
                /// </summary>
                public List<GenvidStreamParameters> Settings;

                /// <summary>
                /// Toggled on when stream creation is done.
                /// </summary> 
                public bool IsInitialized { get; private set; }

                /// <summary>
                /// Specifies logging verbosity. Toggle on for more logging info.
                /// </summary>
                public bool VerboseLog { get; set; }

                /// <summary>
                /// Creates all data streams from the stream parameters.
                /// </summary>
                /// <returns>True if all streams are created correctly, false otherwise.</returns>
                public bool Initialize()
                {
                    bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (!IsInitialized && Settings != null && Settings.Count > 0)
                    {
                        foreach (var stream in Settings)
                        {
                            if (GenvidStreamUtils.CreateStream(stream.Id))
                            {
                                GenvidStreamUtils.SetFrameRate(stream.Id, stream.Framerate);
                            }
                            else
                            {
                                result = false;
                            }
                        }

                        IsInitialized = true;
                    }
#endif
                    return result;
                }

                /// <summary>
                /// Destroys all data streams.
                /// </summary>
                /// <returns>True if all streams are destroyed correctly, false otherwise.</returns>
                public bool Terminate()
                {
                    bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (IsInitialized && Settings != null && Settings.Count > 0)
                    {
                        foreach (var stream in Settings)
                        {
                            result &= GenvidStreamUtils.DestroyStream(stream.Id);
                        }

                        IsInitialized = false;
                    }
#endif
                    return result;
                }

                /// <summary>
                /// Calls the OnStart event assigned to each stream.
                /// </summary>
                public void Start()
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (IsInitialized && Settings.Count > 0)
                    {
                        foreach (var stream in Settings)
                        {
                            if (stream == null)
                            {
                                Debug.LogWarning("Genvid Stream is null! Add a GenvidStreamParameters or decrease the size of the list.");
                            }
                            else
                            {
                                if (stream != null)
                                {
                                    stream.OnStart();
                                }
                            }
                        }
                    }
#endif
                }

                /// <summary>
                /// Calls the submit event of each stream at the same framerate as that stream. 
                /// </summary>
                public void Update()
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (IsInitialized && Settings.Count > 0)
                    {
                        foreach (var stream in Settings)
                        {
                            if (stream != null)
                            {
                                if (stream.Deadline.IsPassed())
                                {
                                    try
                                    {
                                        stream.OnSubmitStream();
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError("Exception during OnSubmitStream: " + e.ToString());
                                    }
                                    stream.Deadline.Next();
                                }
                            }
                        }
                    }
#endif
                }

                /// <summary>
                /// Submits game data on a given stream.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data to submit.</param>
                /// <param name="size">Size of data to submit.</param>
                /// <returns>True if the game data is successfully submitted, false otherwise.</returns>
                public bool SubmitGameData(object streamID, byte[] data, int size)
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (!IsInitialized)
                    {
                        Debug.LogError(String.Format("Unable to submit game data on nonexistent stream '{0}'.", streamID));
                        return false;
                    }

                    var status = GenvidSDK.SubmitGameData(GenvidSDK.GetCurrentTimecode(), streamID.ToString(), data, size);
                    if (GenvidSDK.StatusFailed(status))
                    {
                        Debug.LogError(String.Format("`SubmitGameData` failed with error: {0}.", GenvidSDK.StatusToString(status)));
                        return false;
                    }

                    if (VerboseLog)
                    {
                        Debug.Log(String.Format("Genvid correctly submitted game data: {0}", data));
                    }
#endif

                    return true;
                }

                /// <summary>
                /// Submits an annotation on a given data stream.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data to submit.</param>
                /// <param name="size">Size of data to submit.</param>
                /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
                public bool SubmitAnnotation(object streamID, byte[] data, int size)
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (!IsInitialized)
                    {
                        Debug.LogError(String.Format("Unable to submit annotation on nonexistent stream '{0}'.", streamID));
                        return false;
                    }

                    var status = GenvidSDK.SubmitAnnotation(GenvidSDK.GetCurrentTimecode(), streamID.ToString(), data, size);
                    if (GenvidSDK.StatusFailed(status))
                    {
                        Debug.LogError(String.Format("`SubmitAnnotation` failed with error: {0}", GenvidSDK.StatusToString(status)));
                        return false;
                    }

                    if (VerboseLog)
                    {
                        Debug.Log(String.Format("Genvid correctly submitted annotation: {0}", data));
                    }
#endif
                    return true;
                }

                /// <summary>
                /// Submits game data on a given data stream.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data to submit.</param>
                /// <returns>True if the data is successfully submitted, false otherwise.</returns>
                public bool SubmitGameData(object streamID, byte[] data)
                {
                    return SubmitGameData(streamID, data, data.Length);
                }

                /// <summary>
                /// Submits an annotation on a given data stream.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data to submit.</param>
                /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
                public bool SubmitAnnotation(object streamID, byte[] data)
                {
                    return SubmitAnnotation(streamID, data, data.Length);
                }

                /// <summary>
                /// Submits an game data on a given data stream.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data to submit.</param>
                /// <returns>True if the game data is successfully submitted, false otherwise.</returns>
                public bool SubmitGameData(object streamID, string data)
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (data == null)
                    {
                        Debug.LogError("Unable to handle `null` data. Submitting game data failed.");
                        return false;
                    }

                    var dataAsBytes = Encoding.Default.GetBytes(data);
                    return SubmitGameData(streamID, dataAsBytes);
#else
                return true;
#endif
                }

                /// <summary>
                /// Submits an annotation on a given data stream.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data to submit.</param>
                /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
                public bool SubmitAnnotation(object streamID, string data)
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (data == null)
                    {
                        Debug.LogError("Unable to handle `null` data. Submitting annotation failed.");
                        return false;
                    }

                    var dataAsBytes = Encoding.Default.GetBytes(data);
                    return SubmitAnnotation(streamID, dataAsBytes);
#else
                return true;
#endif
                }

                /// <summary>
                /// Submits game data on a given data stream.
                /// The game-data object is first serialized to JSON.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data object to submit.</param>
                /// <returns>True if the game data is successfully submitted, false otherwise.</returns>
                public bool SubmitGameDataJSON(object streamID, object data)
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (data == null)
                    {
                        Debug.LogError("Unable to handle `null` data. Submitting game data failed.");
                        return false;
                    }

                    var jsonData = GenvidPlugin.SerializeToJSON(data);
                    if (jsonData == null)
                    {
                        Debug.LogError(String.Format("Failed to send game data on stream '{0}' due to a JSON serialization error.", streamID));
                        return false;
                    }

                    return SubmitGameData(streamID, jsonData);
#else
                return true;
#endif
                }

                /// <summary>
                /// Submits an annotation on a given data stream.
                /// The annotation object is first serialized to JSON.
                /// </summary>
                /// <param name="streamID">ID of the stream.</param>
                /// <param name="data">Data object to submit.</param>
                /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
                public bool SubmitAnnotationJSON(object streamID, object data)
                {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    if (data == null)
                    {
                        Debug.LogError("Unable to handle `null` data. Submitting annotation failed.");
                        return false;
                    }

                    var jsonData = GenvidPlugin.SerializeToJSON(data);
                    if (jsonData == null)
                    {
                        Debug.LogError(String.Format("Failed to send annotation on stream '{0}' due to a JSON serialization error.", streamID));
                        return false;
                    }

                    return SubmitAnnotation(streamID, jsonData);
#else
                return true;
#endif
                }
            }
        }
    }

    /// <summary>
    /// Helper class that handles state changes and data submissions for data streams and annotations.
    /// </summary>
    [System.Obsolete("Deprecated. Call `Genvid.Plugin.GenvidStreams` to use Genvid data streams.")]
    public class GenvidStreams : GenvidStreamBase
    {
        /// <summary>
        /// Class representing a stream event as a Unity event.
        /// </summary>
        [Serializable]
        public class StreamEvent : UnityEvent<string>
        {
        }

        /// <summary>
        /// Class containing parameters common to all data streams.
        /// </summary>
        [Serializable]
        public class GenvidStreamElement
        {
            /// <summary>
            /// The stream ID.
            /// </summary>
            [Tooltip("Stream Name")]
            public string Id;

            /// <summary>
            /// The stream framerate.
            /// </summary>
            [Range(0.001f, 60.0f)]
            [Tooltip("Stream Framerate")]
            public float Framerate = 30f;

            /// <summary>
            /// Event to call a single time when the session starts.
            /// </summary>
            [Tooltip("Start Callback")]
            public StreamEvent OnStart;

            /// <summary>
            /// Event to call once every stream frame.
            /// </summary>
            [Tooltip("Stream Callback")]
            public StreamEvent OnSubmitStream;

            /// <summary>
            /// Keeps track of whether the OnStart event was invoked once.
            /// </summary>
            public bool OnStartSubmitted { get; set; }

            /// <summary>
            /// The periodic deadline keeps track of when to invoke
            /// stream-submission callback based on their framerates.
            /// </summary>
            private PeriodicDeadline _Deadline = new PeriodicDeadline();

            /// <summary>
            /// Returns the periodic deadline object.
            /// </summary>
            public PeriodicDeadline Deadline
            {
                get
                {
                    /// Ensure framerate is consistent.
                    _Deadline.Framerate = Framerate;
                    return _Deadline;
                }
            }
        }

        /// <summary>
        /// The stream collection.
        /// </summary>
        public GenvidStreamElement[] Ids;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
    /// Disable warning for other platforms.
#pragma warning disable 414
#endif

        /// <summary>
        /// Toggled on when stream creation is done.
        /// </summary> 
        private bool m_IsCreated = false;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
#pragma warning restore 414
#endif

        /// <summary>
        /// Creates all data streams from the stream parameters.
        /// </summary>
        /// <returns>True if all streams are created correctly, false otherwise.</returns>
        public override bool Create()
        {
            bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK && !m_IsCreated && GenvidSessionManager.IsInitialized)
            {
                foreach (var stream in Ids)
                {
                    var status = GenvidSDK.CreateStream(stream.Id);
                    if (GenvidSDK.StatusFailed(status))
                    {
                        result = false;
                        Debug.LogError("Error while creating the " + stream.Id + " stream: " + GenvidSDK.StatusToString(status));
                    }
                    else
                    {
                        SetFrameRate(stream.Id, stream.Framerate);
                        if (GenvidSessionManager.Instance.ActivateDebugLog)
                        {
                            Debug.Log("Genvid Create data stream named " + stream.Id + " performed correctly.");
                        }
                    }
                }
                m_IsCreated = true;
            }
#endif

            return result;
        }

        /// <summary>
        /// Destroys all data streams.
        /// </summary>
        /// <returns>True if all streams are destroyed correctly, false otherwise.</returns>
        public override bool Destroy()
        {
            bool result = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (GenvidSessionManager.Instance.ActivateSDK && m_IsCreated)
            {
                foreach (var stream in Ids)
                {
                    var status = GenvidSDK.DestroyStream(stream.Id);
                    if (GenvidSDK.StatusFailed(status))
                    {
                        result = false;
                        Debug.LogError("Error while destroying the " + stream + " stream: " + GenvidSDK.StatusToString(status));
                    }
                    else if (GenvidSessionManager.Instance.ActivateDebugLog)
                    {
                        Debug.Log("Genvid Destroy data stream named " + stream.Id + " performed correctly.");
                    }
                }
                m_IsCreated = false;
            }
#endif
            return result;
        }

        /// <summary>
        /// Calls the submit event of each stream at the same framerate as that stream. 
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (m_IsCreated)
            {
                foreach (var stream in Ids)
                {
                    if (!stream.OnStartSubmitted)
                    {
                        if (stream.OnStart != null)
                        {
                            try
                            {
                                stream.OnStart.Invoke(stream.Id);
                                stream.OnStartSubmitted = true;
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Exception during OnStart: " + e.ToString());
                            }
                        }
                    }
                    if (stream.Deadline.IsPassed())
                    {
                        if (stream.OnSubmitStream != null)
                        {
                            try
                            {
                                stream.OnSubmitStream.Invoke(stream.Id);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Exception during OnSubmitStream: " + e.ToString());
                            }
                        }
                        stream.Deadline.Next();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Submits game data on a given stream.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <param name="size">Size of data to submit.</param>
        /// <returns>True if the game data is successfully submitted, false otherwise.</returns>
        public bool SubmitGameData(object streamID, byte[] data, int size)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (!m_IsCreated)
            {
                if (GenvidSessionManager.Instance.ActivateSDK && !GenvidSessionManager.IsInitialized)
                {
                    Debug.LogError("Genvid SDK is not initialized: Unable to submit game data.");
                }
                else
                {
                    Debug.LogError(String.Format("Unable to submit game data on nonexistent stream '{0}'.", streamID));
                }

                return false;
            }

            var status = GenvidSDK.SubmitGameData(GenvidSDK.GetCurrentTimecode(), streamID.ToString(), data, size);
            if (GenvidSDK.StatusFailed(status))
            {
                Debug.LogError(String.Format("`SubmitGameData` failed with error: {0}.", GenvidSDK.StatusToString(status)));
                return false;
            }

            if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log(String.Format("Genvid correctly submitted game data: {0}.", data));
            }
#endif
            return true;
        }

        /// <summary>
        /// Submits an annotation on a given data stream.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <param name="size">Size of data to submit.</param>
        /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
        public bool SubmitAnnotation(object streamID, byte[] data, int size)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (!m_IsCreated)
            {
                if (GenvidSessionManager.Instance.ActivateSDK && !GenvidSessionManager.IsInitialized)
                {
                    Debug.LogError("Genvid SDK is not initialized: Unable to submit annotation.");
                }
                else
                {
                    Debug.LogError(String.Format("Unable to submit annotation on nonexistent stream '{0}'.", streamID));
                }

                return false;
            }

            var status = GenvidSDK.SubmitAnnotation(GenvidSDK.GetCurrentTimecode(), streamID.ToString(), data, size);
            if (GenvidSDK.StatusFailed(status))
            {
                Debug.LogError(String.Format("`SubmitAnnotation` failed with error: {0}.", GenvidSDK.StatusToString(status)));
                return false;
            }

            if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log(String.Format("Genvid correctly submitted annotation: {0}.", data));
            }
#endif
            return true;
        }

        /// <summary>
        /// Submits game data on a given data stream.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the data is successfully submitted, false otherwise.</returns>
        public bool SubmitGameData(object streamID, byte[] data)
        {
            return SubmitGameData(streamID, data, data.Length);
        }

        /// <summary>
        /// Submits an annotation on a given data stream.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
        public bool SubmitAnnotation(object streamID, byte[] data)
        {
            return SubmitAnnotation(streamID, data, data.Length);
        }

        /// <summary>
        /// Submits game data on a given data stream.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the game data is successfully submitted, false otherwise.</returns>
        public bool SubmitGameData(object streamID, string data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (data == null)
            {
                Debug.LogError("Unable to handle `null` data. Submitting game data failed.");
                return false;
            }

            var dataAsBytes = Encoding.Default.GetBytes(data);
            return SubmitGameData(streamID, dataAsBytes);
#else
        return true;
#endif
        }

        /// <summary>
        /// Submits an annotation on a given data stream.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
        public bool SubmitAnnotation(object streamID, string data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (data == null)
            {
                Debug.LogError("Unable to handle `null` data. Submitting annotation failed.");
                return false;
            }

            var dataAsBytes = Encoding.Default.GetBytes(data);
            return SubmitAnnotation(streamID, dataAsBytes);
#else
        return true;
#endif
        }

        /// <summary>
        /// Submits game data on a given data stream.
        /// The game data object is first serialized to JSON.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data object to submit.</param>
        /// <returns>True if the game data is successfully submitted, false otherwise.</returns>
        public bool SubmitGameDataJSON(object streamID, object data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (data == null)
            {
                Debug.LogError("Unable to handle `null` data. Submitting game data failed.");
                return false;
            }

            var jsonData = Genvid.Plugin.GenvidPlugin.SerializeToJSON(data);

            if (jsonData == null)
            {
                Debug.LogError(String.Format("Failed to send game data on stream '{0}' due to a JSON serialization error.", streamID));
                return false;
            }

            return SubmitGameData(streamID, jsonData);
#else
        return true;
#endif
        }

        /// <summary>
        /// Submits an annotation on a given data stream.
        /// The annotation object is first serialized to JSON.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data object to submit.</param>
        /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
        public bool SubmitAnnotationJSON(object streamID, object data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (data == null)
            {
                Debug.LogError("Unable to handle `null` data. Submitting annotation failed.");
                return false;
            }

            var jsonData = Genvid.Plugin.GenvidPlugin.SerializeToJSON(data);

            if (jsonData == null)
            {
                Debug.LogError(String.Format("Failed to send annotation on stream '{0}' due to a JSON serialization error.", streamID));
                return false;
            }

            return SubmitAnnotation(streamID, jsonData);
#else
        return true;
#endif
        }

        /// Submits an annotation on a given data stream.
        /// The annotation object is first serialized to JSON.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data object to submit.</param>
        /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
        [System.Obsolete("This is an obsolete overload. Use `SubmitAnnotationJSON` if you're submitting JSON serializable data.")]
        public bool SubmitAnnotation(object streamID, object data)
        {
            return SubmitAnnotationJSON(streamID, data);
        }

        /// Submits game data on a given data stream.
        /// The game data object is first serialized to JSON.
        /// </summary>
        /// <param name="streamID">ID of the stream.</param>
        /// <param name="data">Data object to submit.</param>
        /// <returns>True if the annotation is successfully submitted, false otherwise.</returns>
        [System.Obsolete("This is an obsolete overload. Use `SubmitGameDataJSON` if you're submitting JSON serializable data.")]
        public bool SubmitGameData(object streamID, object data)
        {
            return SubmitGameDataJSON(streamID, data);
        }

        /// Submits a notification.
        /// </summary>
        /// <param name="notificationID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the notification is successfully submitted, false otherwise.</returns>
        [System.Obsolete("This method was moved to the `GenvidSession` object as notifications are not streams.")]
        public bool SubmitNotification(object notificationID, object data)
        {
            return GenvidSessionManager.Instance.Session.SubmitNotification(notificationID, data);
        }

        /// Submits a notification.
        /// </summary>
        /// <param name="notificationID">ID of the stream.</param>
        /// <param name="data">Data to submit.</param>
        /// <returns>True if the notification is successfully submitted, false otherwise.</returns>
        [System.Obsolete("This method was moved to the `GenvidSession` object as notifications are not streams.")]
        public bool SubmitNotification(object notificationID, string data)
        {
            return GenvidSessionManager.Instance.Session.SubmitNotification(notificationID, data);
        }
    }
}