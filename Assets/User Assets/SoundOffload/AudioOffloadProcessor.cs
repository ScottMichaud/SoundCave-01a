using System;
using System.Collections.Generic;
using System.Threading;

public static class AudioOffloadProcessor
{

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
                //FIXME: Do something other than just duplicating.
                ScratchBuffer[2 * j] += Chunk[j]; //Left
                ScratchBuffer[2 * j + 1] += Chunk[j]; //Right
            }
        }

        Listener.finishSoundProcessing();
    }
}
