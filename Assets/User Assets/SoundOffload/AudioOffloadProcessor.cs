using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

/**
* AudioOffloadProcessor is a static utility class that accepts Offload processing tasks and dumps the results
* into the provided array of floats. This is the class that converts the task into a CPU thread, multiple CPU threads,
* a GPU workload, or whatever else makes sense for the device. AudioOffloadProcessor stands between the Unity AudioOffload
* system (Listeners, Sources, Calls, etc.) and any external libraries that are required (such as the GPU audio pipeline
* that will being designed later).
* <p>
* AudioOffloadProcessor wraps all of the features that it wants to support into a set of methods, which calls the appropriate
* external APIs as necessary.
*
* @author Scott Michaud
* @version 0.1
* @since 2015-07-14
*/

public static class AudioOffloadProcessor
{
    enum Channel { Left, Right };

    /**
    * The entry point for a list of Offload sound calls to be processed. It accepts a list of Offload calls, a reference
    * to an array that it can dump the sound samples in to, and the Offload listener that is supposed to be "hearing"
    * the sounds.
    * <p>
    * The method will return immediately. When the processing is done, it will flip bRenderComplete to true in the
    * supplied Offload listener.
    *
    * @param SoundCalls The list of AudioOffloadCalls to process
    * @param ScratchBuffer The reference to the array where output sound samples are placed
    * @param Listener The AudioOffloadListener that is "hearing" the sounds. Also controls the synchronization lock.
    * @since v0.1
    */
    public static void QueueTask(List<AudioOffloadCall> SoundCalls, float[] ScratchBuffer, AudioOffloadListener Listener)
    {
        ThreadPool.QueueUserWorkItem(state => AudioWorkerJob(SoundCalls, ScratchBuffer, Listener));
    }

    //FIXME: Locked to Stereo Out, Mono In.
    //This method is queued if the task is run in its own, single thread. It will *not* slow the Game or Audio threads.
    private static void AudioWorkerJob(List<AudioOffloadCall> SoundCalls, float[] ScratchBuffer, AudioOffloadListener Listener)
    {
        for (int i = 0; i < ScratchBuffer.Length; i++)
        {
            ScratchBuffer[i] = 0.0F;
        }

        for (int i = 0; i < SoundCalls.Count; i++)
        {
            float[] Chunk = SoundCalls[i].getSampleChunk(ScratchBuffer.Length / 2);

            for (int j = 0; j < ScratchBuffer.Length / 2; j++)
            {
                ScratchBuffer[2 * j] += CPU_CalculateSample(Chunk[j], Listener, SoundCalls[i], Channel.Left); //Left
                ScratchBuffer[2 * j + 1] += CPU_CalculateSample(Chunk[j], Listener, SoundCalls[i], Channel.Right); //Right
            }
        }

        Listener.finishSoundProcessing();
    }

    //This method takes a single sample and calculates distance, direction, etc.
    private static float CPU_CalculateSample(float Sample, AudioOffloadListener Listener, AudioOffloadCall SoundCall, Channel ChannelName)
    {
        float DistanceScale = 1.0f;

        Vector3 Displacement = SoundCall.getLocation() - Listener.getLocation();
        float Distance = Displacement.magnitude;

        Vector3 Direction = Displacement.normalized;
        Vector3 Forward = Listener.getDirection();
        Vector3 Left = new Vector3(-Forward.z, Forward.y, Forward.x); //Rotation Matrix for Left around world.

        float Balance = 1.0F;
        if (ChannelName == Channel.Left)
        {
            Balance = Vector3.Dot(Left, Direction);
        }
        else
        {
            Balance = Vector3.Dot(-Left, Direction);
        }

        Balance += 1.0F;
        Balance *= 0.5F;

        return ((Sample * Balance) / Distance) * DistanceScale;
    }
}
