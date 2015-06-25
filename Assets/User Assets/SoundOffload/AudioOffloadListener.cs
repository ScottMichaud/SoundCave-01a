using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioOffloadListener : MonoBehaviour {
	public bool isEnabled;

	private AudioListener audioDestination;
	private List<AudioOffloadCall> soundCalls; //List of sounds to process.
	private List<AudioOffloadCall> newCalls; //List to add to soundCalls list next chunk.
	//Consider an endCalls list to trigger audioend events, if we need it.
	private double lastFrameTime;
	private int sampleRate;

	// Use this for initialization
	void Start () {
		//Acquire the co-resident AudioListener
		audioDestination = GetComponentInParent<AudioListener>();
		lastFrameTime = Time.unscaledTime;
		sampleRate = AudioSettings.outputSampleRate;
	}
	
	// Update is called once per frame
	void Update () {
		updateClock(Time.unscaledTime);
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		for (int i=0; i < data.Length; i++) {
			data[i] = 0.0f;
		}
	}

	public void addSoundCall (AudioOffloadCall soundCall) {
		if (soundCall.isValid()) {
			newCalls.Add(soundCall);
			soundCall.setLastFrameTime(lastFrameTime);
		}
	}

	void updateClock (double frameTime) {
		lastFrameTime = frameTime;
	}
}
