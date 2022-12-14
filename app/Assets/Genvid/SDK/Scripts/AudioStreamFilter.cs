using UnityEngine;

public class AudioStreamFilter : MonoBehaviour
{
    public delegate void OnAudioFilterDelegate(ref float[] data, int channels);
    public event OnAudioFilterDelegate OnAudioReceivedDataCallback;


    void OnAudioFilterRead(float[] data, int channels)
	{
        if (OnAudioReceivedDataCallback != null)
        {
            OnAudioReceivedDataCallback(ref data, channels);
        }
    }
}
