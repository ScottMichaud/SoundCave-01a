using UnityEngine;
using System.Collections;

public class AudioOffloadCall
{
	private uint startOffset; //Number of samples to delay this call within a chunk.
	private AudioClip audioContent; //The sound payload.
	private Vector3 location; //Where the sound supposedly occured.
	private Vector3 velocity; //Speed of source. (NOTE: Allow on the fly updates.)


	public AudioOffloadCall ()
	{
		startOffset = 0;
	}

	public bool isValid () {
		//Check things like if the clip exists, etc.
		//Nice to have eventually.
		return true;
	}

	public void setLastFrameTime (double Time) {
		startOffset = 0; //FIXME: Implement this offsetting.
	}
}