using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class FinalCutscenePlayer : MonoBehaviour
{
    public PlayableAsset goodTimeline;
    public PlayableAsset neutralTimeline;
    public PlayableAsset badTimeline;

    private void Start()
    {
        string ending = EndingTracker.Instance.GetEndingID();
        var director = GetComponent<PlayableDirector>();

        switch (ending)
        {
            case "GOOD_ENDING":
                director.playableAsset = goodTimeline;
                break;
            case "NEUTRAL_ENDING":
                director.playableAsset = neutralTimeline;
                break;
            case "BAD_ENDING":
                director.playableAsset = badTimeline;
                break;
        }

        director.Play();
    }
}
