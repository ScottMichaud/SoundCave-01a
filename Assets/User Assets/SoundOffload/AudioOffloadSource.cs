using UnityEngine;
using System.Collections.Generic;

public class AudioOffloadSource : MonoBehaviour {

    public AudioClip SelectedClip;

    private List<AudioOffloadListener> OffloadListeners; //Contact all offload listeners, enabled or disabled.
    private Vector3 velocity;
    private Vector3 location;

    void Awake()
    {
        OffloadListeners = new List<AudioOffloadListener>();
        SelectedClip.LoadAudioData();
    }

	// Use this for initialization
	void Start ()
    {
        AudioOffloadListener[] Listeners = FindObjectsOfType<AudioOffloadListener>();
        for (int i = 0; i < Listeners.Length; i++)
        {
            OffloadListeners.Add(Listeners[i]);
        }

        AudioOffloadCall Test = new AudioOffloadCall();
        Test.setAudioClip(SelectedClip);
        Test.setLooping();
        Test.setStartTime(Time.time);
        Test.setLocation(new Vector3(0.0F, 0.0F, 0.0F));
        Test.setVelocity(new Vector3(0.0F, 0.0F, 0.0F));

        OffloadListeners[0].addSoundCall(Test);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public Vector3 getLocation()
    {
        return location;
    }
}
