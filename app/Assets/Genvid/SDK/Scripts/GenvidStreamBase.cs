using UnityEngine;
using System;

namespace Genvid
{
    /// <summary>
    /// Interfaces with elements common to all streams.
    /// </summary>
    [System.Obsolete("Deprecated. Use `Genvid.Plugin.IGenvidPlugin` and `Genvid.Plugin.GenvidParametersBase` to handle your streams.")]
    public interface IGenvidStream
    {
        /// <summary>
        /// Retrieve the framerate of a given stream.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <returns>The framerate.</returns>
        float GetFrameRate(String streamName);

        /// <summary>
        /// Set the granularity of a given stream.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <param name="framerate">The new framerate.</param>
        /// <returns>True if the framerate was successfully updated, false otherwise.</returns>
        bool SetFrameRate(String streamName, float framerate);

        /// <summary>
        /// Retrieve the granularity of a given stream.
        /// Granularity represents the frequency at which the SDK can process incoming data.
        /// Granularity is usually equivalent to the sampling rate or framerate.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <returns>The granularity.</returns>
        float GetGranularity(String streamName);

        /// <summary>
        /// Set the granularity of a given stream.
        /// Granularity represents the frequency at which the SDK can process incoming data.
        /// Granularity is usually equivalent to the sampling rate or framerate.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <param name="granularity">The new granularity.</param>
        /// <returns>True if the granularity was successfully updated, false otherwise.</returns>
        bool SetGranularity(String streamName, float granularity);
    }

    /// <summary>
    /// Abstract class defining helper methods of IGenvidStream.
    /// </summary>
    [System.Obsolete("Deprecated. Use `Genvid.Plugin.IGenvidPlugin` and `Genvid.Plugin.GenvidParametersBase` to handle your streams.")]
    public abstract class GenvidStreamBase : MonoBehaviour, IGenvidStream, IGenvidBase
    {
        /// <summary>
        /// Creates the stream.
        /// </summary>
        /// <returns>True if the stream was successfully created, false otherwise.</returns>
        public abstract bool Create();

        /// <summary>
        /// Destroys the stream.
        /// </summary>
        /// <returns>True if the stream was successfully destroyed, false otherwise.</returns>
        public abstract bool Destroy();

        /// <summary>
        /// Retrieve the framerate of a given stream.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <returns>The framerate. 0.0 if the framerate can't be returned.</returns>
        public float GetFrameRate(String streamName)
        {
            float floatParam = 0.0f;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            floatParam = Plugin.GenvidStreamUtils.GetFrameRate(streamName);
            if(!float.IsNaN(floatParam) && GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Get Frame rate performed correctly.");
            }
#endif
            return floatParam;
        }

        // <summary>
        /// Set the granularity of a given stream.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <param name="framerate">The new framerate.</param>
        /// <returns>True if the framerate was successfully updated, false otherwise.</returns>
        public bool SetFrameRate(String streamName, float framerate)
        {
            bool ret = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            ret = Plugin.GenvidStreamUtils.SetFrameRate(streamName, framerate);

            if (ret && GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid SetFrameRate performed correctly.");
            }
#endif
            return ret;
        }

        /// <summary>
        /// Retrieve the granularity of a given stream.
        /// Granularity represents the frequency at which the SDK can process incoming data.
        /// Granularity is usually equivalent to the sampling rate or framerate.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <returns>The granularity. 0.0 if the granularity can't be returned.</returns>
        public float GetGranularity(String streamName)
        {
            float floatParam = 0.0f;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            floatParam = Plugin.GenvidStreamUtils.GetGranularity(streamName);
            if (!float.IsNaN(floatParam) && GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Get Frame rate performed correctly.");
            }
#endif
            return floatParam;
        }

        /// <summary>
        /// Set the granularity of a given stream.
        /// Granularity represents the frequency at which the SDK can process incoming data.
        /// Granularity is usually equivalent to the sampling rate or framerate.
        /// </summary>
        /// <param name="streamName">ID of the stream.</param>
        /// <param name="granularity">The new granularity.</param>
        /// <returns>True if the granularity was successfully updated, false otherwise.</returns>
        public bool SetGranularity(String streamName, float granularity)
        {
            bool ret = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            ret = Plugin.GenvidStreamUtils.SetGranularity(streamName, granularity);

            if (ret && GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Set Granularity performed correctly.");
            }
#endif
            return ret;
        }
    }
}