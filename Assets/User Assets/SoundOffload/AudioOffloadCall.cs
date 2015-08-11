using UnityEngine;
using System.Collections;

/**
* These are the individual calls that AudioOffloadSource adds to AudioOffloadListener
*
* @author Scott Michaud
* @version 0.1
* @since 2015-06-24
*/


public class AudioOffloadCall
{
	private float startTime; //Time that the sound is supposed to start. Might be mid-frame (think collision at physics substep).
    private int currentSample; //Current sample.
    private bool bLooping; //Does this sound call loop?

	private AudioClip audioContent; //The sound payload.
    private float[] sampleArray; //Samples grabed from audioContent.

	private Vector3 location; //Where the sound supposedly occured.
	private Vector3 velocity; //Speed of source. (NOTE: Allow on the fly updates.)

    public AudioOffloadCall()
    {
        currentSample = 0;
        audioContent = null;
        location = new Vector3();
        velocity = new Vector3();
    }

    public AudioOffloadCall(AudioClip Clip, float StartTime, Vector3 Location, Vector3 Velocity)
    {
        startTime = StartTime;
        currentSample = 0;
        setAudioClip(Clip);
        location = Location;
        velocity = Velocity;
    }

    /**
    * Accepts an AudioClip. 
    *
    * Currently, it requires the audio to be loaded and decoded before creating the sound call.
    * This will probably be done by the source at load time, or further upstream.
    *
    * @param Clip the Unity AudioClip that this call handles
    */

    public void setAudioClip(AudioClip Clip)
    {
        if (audioContent.loadState == AudioDataLoadState.Loaded 
            && audioContent.loadType == AudioClipLoadType.DecompressOnLoad)
        {
            audioContent = Clip;
            sampleArray = new float[audioContent.samples];
            audioContent.GetData(sampleArray, 0);
        }
        else
        {
            audioContent = null;
        }
    }

    public void setStartTime(float Time)
    {
        startTime = Time;
    }

    public float getStartTime()
    {
        return startTime;
    }

    public float[] getSampleChunk(int NumberOfSamples)
    {
        if (!audioContent)
        {
            return new float[1] { 0.0F };
        }

        float[] output = new float[NumberOfSamples];

        for (int i = 0; i < output.Length; i++)
        {
            if (currentSample + i < sampleArray.Length)
            {
                output[i] = sampleArray[currentSample + i];
            }
            else if (bLooping)
            {
                output[i] = sampleArray[currentSample + i % sampleArray.Length]; //Handle multiple loops per output buffer, albeit unlikely.
            }
            else
            {
                output[i] = 0.0F;
            }
        }

        return output;
    }

    public float[] getAllSamples()
    {
        return sampleArray;
    }

    public int getLastSampleNumber()
    {
        if (!audioContent)
        {
            return -1;
        }
        return audioContent.samples;
    }

    public void setVelocity(Vector3 Velocity)
    {
        velocity = Velocity;
    }

    public void setVelocity(float X, float Y, float Z)
    {
        velocity = new Vector3(X, Y, Z);
    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public void setLocation(Vector3 Location)
    {
        location = Location;
    }

    public void setLocation(float X, float Y, float Z)
    {
        location = new Vector3(X, Y, Z);
    }

    public Vector3 getLocation()
    {
        return location;
    }

    public void setLooping()
    {
        bLooping = true;
    }

    public void stopLooping()
    {
        bLooping = false;
    }

    public bool isLooping()
    {
        return bLooping;
    }

    public bool isValid()
    {
        return (audioContent != null);
    }
}