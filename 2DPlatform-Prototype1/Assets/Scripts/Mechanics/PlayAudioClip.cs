using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class allows an audio clip to be played during an animation state.
/// </summary>
public class PlayAudioClip : StateMachineBehaviour
{
    /// <summary>
    /// The point in normalized time where the clip should play.
    /// </summary>
    public float t = 0.5f;
    /// <summary>
    /// If greater than zero, the normalized time will be (normalizedTime % modulus).
    /// This is used to repeat the audio clip when the animation state loops.
    /// </summary>
    public float modulus = 0f;

    /// <summary>
    /// The audio clip to be played.
    /// </summary>
    public List<AudioClip> clip;
    private AudioClip lastClip;
    float last_t = -1f;

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var nt = stateInfo.normalizedTime;
        if (modulus > 0f) nt %= modulus;
        if (nt >= t && last_t < t)
        {
            var newClip = lastClip;
            while (newClip == lastClip) newClip = clip[Random.Range(0, clip.Count)];
            AudioSource.PlayClipAtPoint(newClip, animator.transform.position);
            lastClip = newClip;
        }
        last_t = nt;
    }
}
