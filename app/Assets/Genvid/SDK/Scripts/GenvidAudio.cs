using UnityEngine;
using System;
using GenvidSDKCSharp;

public class GenvidAudio : GenvidStreamBase
{
    public enum AudioMode
    {
        None,
        WASAPI,
        Unity
    }

    [SerializeField]
    string m_StreamName;
    [SerializeField]
    GenvidSDK.AudioFormat m_AudioFormat = GenvidSDK.AudioFormat.S16LE;
    [SerializeField]
    AudioMode m_AudioMode = AudioMode.Unity;

    AudioListener m_AudioListener;
    AudioStreamFilter m_AudioStreamFilter;

    private bool m_ChannelNumberSet = false;

    public string StreamName
    {
        get { return m_StreamName; }
        private set { m_StreamName = value; }
    }

    public GenvidSDK.AudioFormat AudioFormat
    {
        get
        {
            int paramReceived = 0;
            var gvStatus = GenvidSDK.GetParameter(m_StreamName, "audio.format", ref paramReceived);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to get the audio format parameter : " + GenvidSDK.StatusToString(gvStatus));
                return GenvidSDK.AudioFormat.Unknown;
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Get Parameter audio format performed correctly.");
            }
            return (GenvidSDK.AudioFormat)paramReceived;
        }
        set
        {
            var gvStatus = GenvidSDK.SetParameter(m_StreamName, "audio.format", (int)value);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to set the audio format parameter : " + GenvidSDK.StatusToString(gvStatus));
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Set Parameter audio format performed correctly.");
            }
        }
    }

    public int AudioChannels
    {
        get
        {
            int paramReceived = 0;
            var gvStatus = GenvidSDK.GetParameter(m_StreamName, "audio.channels", ref paramReceived);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to get the audio channels parameter : " + GenvidSDK.StatusToString(gvStatus));
                return -1;
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Get Parameter audio channel performed correctly.");
            }
            return paramReceived;
        }
        set
        {
            var gvStatus = GenvidSDK.SetParameter(m_StreamName, "audio.channels", value);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to set the audio channels parameter : " + GenvidSDK.StatusToString(gvStatus));
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Set Parameter audio channel performed correctly.");
            }
        }
    }

    public int AudioRate
    {
        get
        {
            int paramReceived = 0;
            var gvStatus = GenvidSDK.GetParameter(m_StreamName, "audio.rate", ref paramReceived);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to get the audio rate parameter : " + GenvidSDK.StatusToString(gvStatus));
                return -1;
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Get Parameter audio rate performed correctly.");
            }
            return paramReceived;
        }
        set
        {
            var gvStatus = GenvidSDK.SetParameter(m_StreamName, "audio.rate", value);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to set the audio rate parameter : " + GenvidSDK.StatusToString(gvStatus));
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Set Parameter audio rate performed correctly.");
            }
        }
    }

    public int AudioGranularity
    {
        get
        {
            int paramReceived = 0;
            var gvStatus = GenvidSDK.GetParameter(m_StreamName, "granularity", ref paramReceived);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to get the audio granularity parameter : " + GenvidSDK.StatusToString(gvStatus));
                return -1;
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Get Parameter audio granularity performed correctly.");
            }
            return paramReceived;
        }
        set
        {
            var gvStatus = GenvidSDK.SetParameter(m_StreamName, "granularity", value);
            if (GenvidSDK.StatusFailed(gvStatus))
            {
                Debug.LogError("Error while trying to set the audio granularity parameter : " + GenvidSDK.StatusToString(gvStatus));
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
                Debug.Log("Genvid Set Parameter audio granularity performed correctly.");
            }
        }
    }

    public new bool Create()
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

            if(m_AudioMode != AudioMode.None)
            {
                if (GenvidSessionManager.Instance.Session.VideoStream == null)
                {
                    Debug.LogError("Error while Accessing Video framerate from " + m_StreamName + " stream.");
                    return false;
                }
            }

            if (m_AudioMode == AudioMode.WASAPI)
			{
                status = GenvidSDK.SetParameter(m_StreamName, "Audio.Source.WASAPI", 1);
				if (GenvidSDK.StatusFailed(status))
                {
					Debug.LogError("Error while setting the parameter for the audio : " + GenvidSDK.StatusToString(status));
                    return false;
				}
                else if (GenvidSessionManager.Instance.ActivateDebugLog)
                {
                    Debug.Log("Genvid Set Parameter Wasapi performed correctly.");
                }
            }
            else if (m_AudioMode == AudioMode.Unity)
            {
                m_AudioListener = FindObjectOfType<AudioListener>();
                if (m_AudioListener == null)
                {
                    Debug.LogWarning("No AudioListener was found. Adding AudioListener...");
                    m_AudioListener = gameObject.AddComponent<AudioListener>();
                }

                m_AudioStreamFilter = m_AudioListener.GetComponent<AudioStreamFilter>();
                if (m_AudioStreamFilter == null)
                {
                    m_AudioStreamFilter = m_AudioListener.gameObject.AddComponent<AudioStreamFilter>();
                }

                var config = AudioSettings.GetConfiguration();
                AudioRate = config.sampleRate;
                AudioFormat = m_AudioFormat;

                Debug.Log("DSP buffer size is " + config.dspBufferSize);

                m_AudioStreamFilter.OnAudioReceivedDataCallback += OnAudioReceivedDataCallback;
            }
		}
    #endif
        return true;
    }

	public new void Destroy()
    {
    #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if(GenvidSessionManager.Instance.ActivateSDK)
        {
            if (m_AudioStreamFilter != null)
            {
                m_AudioStreamFilter.OnAudioReceivedDataCallback -= OnAudioReceivedDataCallback;
            }

            var status = GenvidSDK.DestroyStream(m_StreamName);
            if (GenvidSDK.StatusFailed(status))
            {
               Debug.LogError("Error while destroying the " + m_StreamName + " stream: " + GenvidSDK.StatusToString(status));
            }
            else if (GenvidSessionManager.Instance.ActivateDebugLog)
            {
               Debug.Log("Genvid Destroy audio stream named " + m_StreamName + " performed correctly.");
            }
        }
    #endif
    }

    private void OnAudioReceivedDataCallback(ref float[] data, int channels)
    {
        AudioFilterRead(ref data, channels);
    }

    void AudioFilterRead(ref float[] data, int channels)
    {
        if (!m_ChannelNumberSet)
        {
            AudioChannels = channels;
            m_ChannelNumberSet = true;
        }

        if (m_AudioMode == AudioMode.Unity)
        {
            long tc = GenvidSDK.GetCurrentTimecode();

            if (m_AudioFormat == GenvidSDK.AudioFormat.S16LE)
            {
                var shortData = ConvertTo16Bits(ref data);
                var status = GenvidSDK.SubmitAudioData(tc, m_StreamName, ref shortData);
                if (GenvidSDK.StatusFailed(status))
                {
                    Debug.LogError("Error while submitting the audio data to the " + m_StreamName + " stream: " + GenvidSDK.StatusToString(status));
                }
                else if (GenvidSessionManager.Instance.ActivateDebugLog)
                {
                    Debug.Log("Genvid Submit audio data performed correctly.");
                }
            }
            else if (m_AudioFormat == GenvidSDK.AudioFormat.F32LE)
            {
                var status = GenvidSDK.SubmitAudioData(tc, m_StreamName, ref data);
                if (GenvidSDK.StatusFailed(status))
                {
                    Debug.LogError("Error while submitting the audio data to the " + m_StreamName + " stream: " + GenvidSDK.StatusToString(status));
                }
                else if (GenvidSessionManager.Instance.ActivateDebugLog)
                {
                    Debug.Log("Genvid Submit audio data performed correctly.");
                }
            }
            else
            {
                Debug.LogError("Unknown Audio format.");
            }
        }
    }

    short[] ConvertTo16Bits(ref float[] dataSource)
	{
		short[] dataDest = new short[dataSource.Length];

		for (int i = 0; i < dataSource.Length; i++)
		{
			int value = (int)(dataSource[i] * 32768.0f);
			// Clamp the value to avoid audio glitch
			dataDest[i] = (short)Math.Min(Math.Max(value, -32768), 32767);
		}

		return dataDest;
	}

    bool IsEnumDefined<T>(string enumValue)
    {
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            if(String.Equals(value.ToString(), enumValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    void ParseCommandLine()
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

                if(!isDefined)
                {
                    Debug.LogError("Use one of this AudioMode: " + String.Join(", ", Enum.GetNames(typeof(AudioMode))) + ".");
                }
                break;
            }
        }
    }
}
