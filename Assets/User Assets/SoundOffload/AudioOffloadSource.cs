using UnityEngine;
using System.Collections.Generic;

/**
* AudioOffloadSource is a MonoBehaviour component that you attach to objects that you want to emit
* sound. They will be sent to every AudioOffloadListener, regardless of whether they're active or not.
* This will probably be subclassed by various types of placeable objects, based on how they actually emit
* the sound (immediate, delayed, triggered, randomization, and so forth).
* <p>
* AudioOffloadSource currently just pushes a pre-defined AudioClip to World (0, 0, 0) for testing purposes.
*
* @author Scott Michaud
* @version 0.1
* @since 2015-06-24
*/
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

    void FixedUpdate()
    {
        Transform transform = GetComponentInParent<Transform>();

        velocity = (transform.position - location) / Time.fixedDeltaTime;
        location = transform.position;
    }

    /**
    * Getter for the source's current velocity
    *
    * @returns The source's current velocity
    */
    public Vector3 getVelocity()
    {
        return velocity;
    }

    /**
    * Getter for the source's current location
    *
    * @returns The source's current location
    */
    public Vector3 getLocation()
    {
        return location;
    }
}
