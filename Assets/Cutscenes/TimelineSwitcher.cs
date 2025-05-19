using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineSwitcher : MonoBehaviour
{
    public PlayableAsset firstTimeline;
    public PlayableAsset secondTimeline;

    public GameObject secondDirectorObject;         // Object to activate
    public PlayableDirector secondDirector;         // Second timeline controller
    public float waitBeforeSecond = 3f;             // Delay after first timeline ends
    public float waitAfterSecondTimeline = 1f;      // Delay before starting second director

    public PlayableAsset neutralTimeline;           // Neutral ending timeline
    public PlayableAsset badTimeline;               // Bad ending timeline

    private PlayableDirector director;

    void Start()
    {
        director = GetComponent<PlayableDirector>();
        string ending = EndingTracker.Instance.GetEndingID();

        Debug.Log("🟢 TimelineSwitcher Start() — Ending: " + ending);

        if (ending == "GOOD_ENDING")
        {
            if (firstTimeline != null)
            {
                director.playableAsset = firstTimeline;
                director.Play();
                Debug.Log("▶️ Playing first GOOD timeline.");
            }

            StartCoroutine(SwitchSequence());
        }
        else if (ending == "NEUTRAL_ENDING" && neutralTimeline != null)
        {
            director.playableAsset = neutralTimeline;
            director.Play();
            Debug.Log("🟡 Playing NEUTRAL timeline.");
            StartCoroutine(SwitchSequence()); // ✅ Run coroutine for post-cutscene activation
        }
        else if (ending == "BAD_ENDING" && badTimeline != null)
        {
            director.playableAsset = badTimeline;
            director.Play();
            Debug.Log("🔴 Playing BAD timeline.");
            StartCoroutine(SwitchSequence()); // ✅ Same here
        }
    }
    IEnumerator SwitchSequence()
    {
        Debug.Log("⏳ Waiting for first timeline to end...");

        // Wait for first timeline to finish
        yield return new WaitUntil(() => director.state != PlayState.Playing);

        Debug.Log("✅ First timeline finished. Waiting before second...");

        // Optional pause after first
        yield return new WaitForSeconds(waitBeforeSecond);

        // Play second timeline on same director (if any)
        if (secondTimeline != null)
        {
            director.playableAsset = secondTimeline;
            director.Play();
            Debug.Log("▶️ Playing second timeline.");

            // Wait for second timeline to finish
            yield return new WaitUntil(() => director.state != PlayState.Playing);
        }
        else
        {
            Debug.Log("ℹ️ No second timeline assigned. Skipping to second director.");
        }

        // Optional buffer before activating second director
        yield return new WaitForSeconds(waitAfterSecondTimeline);

        if (secondDirectorObject != null)
        {
            secondDirectorObject.SetActive(true);
            Debug.Log("✅ Activated secondDirectorObject: " + secondDirectorObject.name);
        }

        if (secondDirector != null)
        {
            secondDirector.Play();
            Debug.Log("🎬 Started secondDirector: " + secondDirector.name);
        }
        else
        {
            Debug.LogWarning("⚠️ secondDirector is null!");
        }
    }
}
