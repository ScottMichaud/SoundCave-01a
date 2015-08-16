using UnityEngine;
using System.Collections.Generic;

/**
* AudioOffloadListener is a MonoBehaviour component that you attach to objects, which will likely
* be either your player object or your camera. They have a list of sound calls that they send to
* be processed every Audio Thread chunk. Think of an Audio Thread chunk like a "frame" for your
* sound card, but it's not tied to video frame rate. It's typically ~43 chunks per second, locked. 
* <p>
* AudioOffloadListener adds the processed chunk to Unity's chunk just before it reaches the speaker.
* It also holds relevant information, such as where it is, how fast it is moving, and which way it
* is facing.
*
* @author Scott Michaud
* @version 0.1
* @since 2015-06-24
*/
public class AudioOffloadListener : MonoBehaviour {
	public bool isEnabled;

    private bool bRenderComplete;
    private float[] outBuffer;
    private float[] scratchBuffer;

	private AudioListener audioDestination;
	private List<AudioOffloadCall> soundCalls; //List of sounds to process.
	private List<AudioOffloadCall> newCalls; //List to add to soundCalls list next chunk.

    //Consider an endCalls list to trigger audioend events, if we need it.

    private Vector3 location;
    private Vector3 velocity;
    private Vector3 direction;

	private float lastFrameTime;
	private int sampleRate;

    // Use in place of a constructor
    void Awake()
    {
        outBuffer = new float[0];
        soundCalls = new List<AudioOffloadCall>();
        newCalls = new List<AudioOffloadCall>();
    }

	// Use this for initialization
	void Start ()
    {
		//Acquire the co-resident AudioListener
        //TODO: Eventually get the active listener to allow multiple listeners.
		audioDestination = GetComponentInParent<AudioListener>();
		lastFrameTime = Time.unscaledTime;
		sampleRate = AudioSettings.outputSampleRate;
        bRenderComplete = true;
	}
	
	// Update is called once per video frame
	void Update ()
    {
		
	}

    //Called once per physics frame, which is a more reliable way to get character and object
    //position and velocity.
    void FixedUpdate ()
    {
        setLastFrameTime(Time.unscaledTime);
        Transform transform = GetComponentInParent<Transform>();

        velocity = (transform.position - location) / Time.fixedDeltaTime;
        location = transform.position;
        direction = transform.forward;
    }

   /**
   * Callback for each audio frame.
   *
   * Tasks: Juggle buffers, update each call property as necessary, and queue new computation.
   * We're currently going to try a double-buffer method. More could be added if it makes sense for the compute device.
   * Ex: Easy CPU multicore is 1 buffer per core. GPUs could benefit from triple-buffer. Less is lower latency, though.
   * Reminder that not-offloaded audio calls are not buffered. They enter and leave with data unmodified and instantly.
   *
   * @param data The buffer containing Unity audio mixdown, which will be output when function terminates
   * @param channels The number of speakers to output to. 2 is Stereo.
   */
	void OnAudioFilterRead (float[] data, int channels)
    {
        //If the offloaded sound isn't finished yet, fail to replaying the previous buffer.
        if (bRenderComplete)
        {
            float[] temp = outBuffer;
            outBuffer = scratchBuffer;
            scratchBuffer = temp;
        }

        if (outBuffer.Length == data.Length) //Either first queue, or buffer size changed.
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] += outBuffer[i]; //Add our audio to Unity's buffer.
            }
        }
        else
        {
            outBuffer = new float[data.Length];
            scratchBuffer = new float[data.Length];
        }

        //Flag onPlay callbacks to be executed for each new call, if necessary.
        //Don't run the calls here, because this is on the Audio thread. Flip a boolean.

        if (newCalls.Count > 0)
        {
            soundCalls.AddRange(newCalls);
            newCalls.Clear();
        }

        for (int i = 0; i < soundCalls.Count; i++)
        {
            soundCalls[i].setVelocity();
            soundCalls[i].setLocation();
        }

        if (bRenderComplete)
        {
            bRenderComplete = false;
            enqueueSoundProcessing(soundCalls, scratchBuffer, this);
        }
	}

    /**
    * Sends the current sound calls to the processor.
    * 
    * @param SoundCalls The list of offloaded AudioOffloadCalls.
    * @param ScratchBuffer Reference to the buffer where the result will be placed.
    * @param Listener The AudioOffloadListener that is supposed to be hearing the SoundCalls.
    */
    public void enqueueSoundProcessing(List<AudioOffloadCall> SoundCalls, float[] ScratchBuffer, AudioOffloadListener Listener)
    {
        AudioOffloadProcessor.QueueTask(SoundCalls, ScratchBuffer, Listener);
    }

    /**
    * Called by whatever thread completes the processing, which is monitored by the Audio Thread to
    * know that the buffer is ready to add to Unity's audio pipe. Bools are small enough that
    * read/writes are atomic on all platforms (guaranteed by C# spec itself).
    */
    public void finishSoundProcessing()
    {
        bRenderComplete = true;
    }

    /**
    * Adds an AudioOffloadCall to this listener. It is dumped to a queue so that multiple calls will
    * be added all at once. This is for performance.
    * 
    * @param SoundCall The offloaded AudioOffloadCall.
    */
    public void addSoundCall (AudioOffloadCall SoundCall)
    {
		if (SoundCall.isValid())
        {
			newCalls.Add(SoundCall);
            //This will be set by the source instead.
			//SoundCall.setStartTime(lastFrameTime);
		}
	}

	private void setLastFrameTime (double FrameTime)
    {
		lastFrameTime = (float) FrameTime;
	}

    /**
    * Setter for velocity
    * 
    * @param Velocity The listener velocity at this point in time.
    */
    public Vector3 getVelocity()
    {
        return velocity;
    }

    /**
    * Setter for location
    * 
    * @param Location The listener location at this point in time.
    */
    public Vector3 getLocation()
    {
        return location;
    }

    /**
    * Setter for direction
    * 
    * @param Direction The listener direction at this point in time.
    */
    public Vector3 getDirection()
    {
        return direction;
    }
}
