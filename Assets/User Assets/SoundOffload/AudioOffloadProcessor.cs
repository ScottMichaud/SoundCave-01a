using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

public static class AudioOffloadProcessor
{
    enum Channel { Left, Right };
    private static ManualResetEvent done;

    public static void QueueTask(List<AudioOffloadCall> SoundCalls, float[] ScratchBuffer, AudioOffloadListener Listener)
    {
        ThreadPool.QueueUserWorkItem(state => AudioWorkerJob(SoundCalls, ScratchBuffer, Listener));
    }

    //FIXME: Locked to Stereo Out, Mono In.

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
