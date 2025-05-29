using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class CutsceneController : MonoBehaviour
{
    public PlayableDirector director;
    public PlayableAsset goodTimeline;
    public PlayableAsset neutralTimeline;
    public PlayableAsset badTimeline;
    public PlayableAsset secondTimeline;

    public GameObject secondDirectorObject;
    public PlayableDirector secondDirector;
    public float waitBeforeSecond = 3f;
    public float waitAfterSecond = 1f;

    void Start()
    {
        string ending = EndingTracker.Instance.GetEndingID();
        PlayableAsset chosenTimeline = null;

        switch (ending)
        {
            case "GOOD_ENDING": chosenTimeline = goodTimeline; break;
            case "NEUTRAL_ENDING": chosenTimeline = neutralTimeline; break;
            case "BAD_ENDING": chosenTimeline = badTimeline; break;
        }

        director.playableAsset = chosenTimeline;
        director.Play();
        StartCoroutine(SwitchSequence());
    }

    IEnumerator SwitchSequence()
    {
        yield return new WaitUntil(() => director.state != PlayState.Playing);
        yield return new WaitForSeconds(waitBeforeSecond);

        if (secondTimeline != null)
        {
            director.playableAsset = secondTimeline;
            director.Play();
            yield return new WaitUntil(() => director.state != PlayState.Playing);
        }

        yield return new WaitForSeconds(waitAfterSecond);

        if (secondDirectorObject != null) secondDirectorObject.SetActive(true);
        if (secondDirector != null) secondDirector.Play();
    }
}