using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class IntroStarter : MonoBehaviour
{
     public PlayableDirector director;

    void Start()
    {
        director.Play();
    }
}
