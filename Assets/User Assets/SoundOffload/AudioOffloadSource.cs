using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioOffloadSource : MonoBehaviour {

	private List<AudioOffloadListener> OffloadListeners; //Contact all offload listeners, enabled or disabled.
    private Vector3 velocity;
    private Vector3 location;

	// Use this for initialization
	void Start ()
    {
	
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
