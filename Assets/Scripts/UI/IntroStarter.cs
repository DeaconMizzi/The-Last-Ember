using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class IntroStarter : MonoBehaviour
{
    public PlayableDirector director;
    public PlayerMovement playerMovement;

    public GameObject introCanvas;

    void Start()
    {
        if (director != null)
        {
            director.stopped += OnTimelineFinished;
            director.Play();
            GameStateController.Instance?.SetCutsceneState(true);

        }
        if (playerMovement != null)
        {
            playerMovement.canMove = false; // freeze movement logic
            Animator anim = playerMovement.GetComponent<Animator>();
            anim.SetBool("isMoving", false);
            anim.SetFloat("moveX", 0);
            anim.SetFloat("moveY", 1); // 1 = facing up (north)
        }
    }

    void OnTimelineFinished(PlayableDirector pd)
    {
         if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }
        if (introCanvas != null)
        {
            GameStateController.Instance?.SetCutsceneState(false);

            introCanvas.SetActive(false);
            Debug.Log("Intro canvas disabled after timeline ended.");
        }
    }
}
