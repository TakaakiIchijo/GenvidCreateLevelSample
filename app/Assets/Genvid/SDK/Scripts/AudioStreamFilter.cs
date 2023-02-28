using UnityEngine;

namespace Genvid
{
    /// <summary>
    /// Use this class to assign a callback to
    /// trigger when audio data is received from a listener.
    /// </summary>
    public class AudioStreamFilter : MonoBehaviour
    {
        /// <summary>
        /// Delegate declaration. Refers to a callback triggered on reception of new audio data.
        /// </summary>
        /// <param name="data">The new audio data.</param>
        /// <param name="channels">The number of channels.</param>
        public delegate void OnAudioFilterDelegate(float[] data, int channels);

        /// <summary>
        /// Callback to trigger on reception of new audio data.
        /// </summary>
        public event OnAudioFilterDelegate OnAudioReceivedDataCallback;

        /// <summary>
        /// Invokes the callback on reception of new audio data.
        /// </summary>
        /// <param name="data">The new audio data.</param>
        /// <param name="channels">The number of channels.</param>
        public void OnAudioFilterRead(float[] data, int channels)
        {
            if (OnAudioReceivedDataCallback != null)
            {
                OnAudioReceivedDataCallback(data, channels);
            }
        }
    }
}