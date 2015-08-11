using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioOffloadListener : MonoBehaviour {
	public bool isEnabled;

	private AudioListener audioDestination;
	private List<AudioOffloadCall> soundCalls; //List of sounds to process.
	private List<AudioOffloadCall> newCalls; //List to add to soundCalls list next chunk.

    //Consider an endCalls list to trigger audioend events, if we need it.

    private Vector3 location;
    private Vector3 velocity;

	private float lastFrameTime;
	private int sampleRate;

	// Use this for initialization
	void Start ()
    {
		//Acquire the co-resident AudioListener
		audioDestination = GetComponentInParent<AudioListener>();
		lastFrameTime = Time.unscaledTime;
		sampleRate = AudioSettings.outputSampleRate;
	}
	
	// Update is called once per video frame
	void Update ()
    {
		setLastFrameTime(Time.unscaledTime);

        if (audioDestination)
        {
            velocity = (audioDestination.transform.position - location) / Time.deltaTime;
            location = audioDestination.transform.position;
        }
	}

    // This is called once per audio frame
    // Tasks: Swap buffer, update each call property if necessary, and queue new computation.
	void OnAudioFilterRead (float[] data, int channels)
    {
		for (int i=0; i < data.Length; i++)
        {
			data[i] = data[i];
		}
	}

	public void addSoundCall (AudioOffloadCall SoundCall)
    {
		if (SoundCall.isValid())
        {
			newCalls.Add(SoundCall);
			SoundCall.setStartTime(lastFrameTime);
		}
	}

	void setLastFrameTime (double FrameTime)
    {
		lastFrameTime = (float) FrameTime;
	}
}
