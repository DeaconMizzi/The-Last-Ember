using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestPopupUI : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_Text popupText;
    public float displayTime = 2f;

    private Coroutine currentPopup;

    public void ShowPopup(string message)
    {
        if (popupPanel == null || popupText == null) return;

        popupText.text = message;
        popupPanel.SetActive(true);

        if (currentPopup != null)
            StopCoroutine(currentPopup);

        currentPopup = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        popupPanel.SetActive(false);
        currentPopup = null;
    }
}
