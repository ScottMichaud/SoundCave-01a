using UnityEngine;

/**
* These are the individual calls that AudioOffloadSource adds to AudioOffloadListener. They are
* messages that contain everything the source wants the Listener to know for that one sound
* instance. For instance, where did this sound come from? What does it sound like? What velocity
* does it have (if we care about Doppler Shift)? Etc.
* <p>
* If necessary, we can also provide it with delegates to call on the next main thread Update()
* after various events. Ex: Functions to call when it starts being played, when it finishes playing,
* after each loop, etc.
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
    private AudioOffloadSource source; //Source that caused this call

    /**
    * Constructor
    *
    * @since 0.1
    */
    public AudioOffloadCall()
    {
        currentSample = 0;
        audioContent = null;
        location = new Vector3();
        velocity = new Vector3();
    }

    /**
    * Constructor
    *
    * @param Clip The Unity AudioClip that will provide the base sound.
    * @param StartTime When the clip should begin, if delayed start.
    * @param Location Where the sound is currently coming from. Can be updated after start.
    * @param Velocity How fast the sound is moving. Can be updated after start.
    * @since 0.1
    */
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
    * @param Clip The Unity AudioClip that this call handles
    * @since 0.1
    */

    public void setAudioClip(AudioClip Clip)
    {
        if (Clip.loadState == AudioDataLoadState.Loaded 
            && Clip.loadType == AudioClipLoadType.DecompressOnLoad)
        {
            audioContent = Clip;

            if (AudioSettings.outputSampleRate == Clip.frequency)
            {
                //No conversion necessary.
                sampleArray = new float[audioContent.samples];
                audioContent.GetData(sampleArray, 0);
            }
            else
            {
                float[] toConvert = new float[audioContent.samples];
                audioContent.GetData(toConvert, 0);

                int newSize = Mathf.CeilToInt((float)audioContent.samples * ((float)AudioSettings.outputSampleRate / (float)Clip.frequency));
                sampleArray = new float[newSize];

                sampleArray[0] = toConvert[0];
                sampleArray[newSize - 1] = toConvert[toConvert.Length - 1];


                for (int i = 1; i < (newSize - 2); i++)
                {
                    float position = ((float)i / (float)(newSize - 1)) * (float)(toConvert.Length - 1);

                    //Look forward and back... if position lies on an integer this will be the same.
                    float firstSample = toConvert[Mathf.FloorToInt(position)];
                    float secondSample = toConvert[Mathf.CeilToInt(position)];

                    //The distance between first and second will linearly weight between the two samples.
                    float secondWeight = position - (Mathf.Floor(position));
                    float firstWeight = 1.0F - secondWeight;

                    sampleArray[i] = (firstWeight * firstSample) + (secondWeight * secondSample);
                }
            }
        }
        else
        {
            audioContent = null;
        }
    }

    /**
    * Setter for the start time.
    *
    * @param Time The start time (probably in Unity seconds).
    */

    public void setStartTime(float Time)
    {
        startTime = Time;
    }

    /**
    * Getter for the start time.
    *
    * @return The start time.
    */
    public float getStartTime()
    {
        return startTime;
    }

    /**
    * Getter for the next chunk of sound samples (received from the attached AudioClip) to be played.
    *
    * @return An array of sound samples.
    */
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
                output[i] = sampleArray[(currentSample + i) % sampleArray.Length]; //Handle multiple loops per output buffer, albeit unlikely.
            }
            else
            {
                output[i] = 0.0F;
            }
        }

        if (bLooping)
        {
            currentSample = (currentSample + NumberOfSamples) % sampleArray.Length;
        }
        else
        {
            currentSample = (currentSample + NumberOfSamples);
        }

        return output;
    }

    /**
    * Getter for the AudioClip's entire sound sample array.
    *
    * @return Every sound sample in AudioClip.
    */
    public float[] getAllSamples()
    {
        return sampleArray;
    }

    /**
    * States the number of samples in the attached AudioClip.
    *
    * @return Total number of samples in clip.
    */
    public int getNumberOfSamples()
    {
        if (!audioContent)
        {
            return -1;
        }
        return audioContent.samples;
    }

    /**
    * Setter for velocity
    * 
    * @param Velocity The desired velocity at this point in time.
    */
    public void setVelocity(Vector3 Velocity)
    {
        velocity = Velocity;
    }

    /**
    * Setter for velocity
    * 
    * @param X The x component of the velocity
    * @param Y The y component of the velocity
    * @param Z The z component of the velocity
    */
    public void setVelocity(float X, float Y, float Z)
    {
        velocity = new Vector3(X, Y, Z);
    }

    /**
    * Tells the sound call to acquire its velocity from its source.
    */
    public void setVelocity()
    {
        if (source)
        {
            velocity = source.getVelocity();
        }
    }

    /**
    * Tells the sound call to acquire its location from its source.
    */
    public void setLocation()
    {
        if (source)
        {
            location = source.getLocation();
        }
    }

    /**
    * Getter for the call's current velocity
    *
    * @returns Its current velocity
    */
    public Vector3 getVelocity()
    {
        return velocity;
    }

    /**
    * Setter for location
    * 
    * @param Location The desired location at this point in time.
    */
    public void setLocation(Vector3 Location)
    {
        location = Location;
    }

    /**
    * Setter for location
    * 
    * @param X The desired X component of its location.
    * @param Y The desired Y component of its location.
    * @param Z The desired Z component of its location.
    */
    public void setLocation(float X, float Y, float Z)
    {
        location = new Vector3(X, Y, Z);
    }

    /**
    * Getter for the call's current location
    *
    * @returns Its current location
    */
    public Vector3 getLocation()
    {
        return location;
    }

    /**
    * Makes the sound call loop upon completion
    */
    public void setLooping()
    {
        bLooping = true;
    }

    /**
    * Makes the sound call stop looping when it's complete
    */
    public void stopLooping()
    {
        bLooping = false;
    }

    /**
    * States whether the sound call will loop when it's complete
    *
    * @return true if the sound call will loop
    */
    public bool isLooping()
    {
        return bLooping;
    }

    /**
    * States whether the sound call is valid
    *
    * @return false if the sound call is corrupt in some way
    */
    public bool isValid()
    {
        return (audioContent != null);
    }
}