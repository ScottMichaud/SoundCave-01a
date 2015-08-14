using UnityEngine;
using System.Collections.Generic;

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
		audioDestination = GetComponentInParent<AudioListener>();
		lastFrameTime = Time.unscaledTime;
		sampleRate = AudioSettings.outputSampleRate;
        bRenderComplete = true;
	}
	
	// Update is called once per video frame
	void Update ()
    {
		
	}

    void FixedUpdate ()
    {
        setLastFrameTime(Time.unscaledTime);

        if (audioDestination)
        {
            velocity = (audioDestination.transform.position - location) / Time.fixedDeltaTime;
            location = audioDestination.transform.position;
        }
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

    public void enqueueSoundProcessing(List<AudioOffloadCall> SoundCalls, float[] ScratchBuffer, AudioOffloadListener Listener)
    {
        AudioOffloadProcessor.QueueTask(SoundCalls, ScratchBuffer, Listener);
        //Listener.finishSoundProcessing();
    }

    public void finishSoundProcessing()
    {
        bRenderComplete = true;
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
