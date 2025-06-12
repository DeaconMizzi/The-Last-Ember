using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform heartContainer;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    private List<Image> hearts = new List<Image>();

    public void UpdateHearts(int current, int max)
    {
        Debug.Log($"[HeartUI] Updating Hearts: {current} / {max}");

        // Ensure we have enough heart slots
        while (hearts.Count < max)
        {
            GameObject heart = Instantiate(heartPrefab);
            heart.transform.SetParent(heartContainer, false);
            heart.SetActive(true);

            Image heartImage = heart.GetComponent<Image>();
            if (heartImage != null)
            {
                hearts.Add(heartImage);
            }
            else
            {
                Debug.LogWarning("Spawned heart prefab is missing an Image component.");
            }
        }

        while (hearts.Count > max)
        {
            Destroy(hearts[hearts.Count - 1].gameObject);
            hearts.RemoveAt(hearts.Count - 1);
        }

        // Update heart sprites
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].sprite = i < current ? fullHeart : emptyHeart;
        }
    }
}

