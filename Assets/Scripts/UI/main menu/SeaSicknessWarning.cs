using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SeaSicknessWarning : MonoBehaviour
{
    [SerializeField] private CanvasGroup imageCanvasGroup;

    // Survives scene reloads within the same app session
    private static bool s_hasShownWarning = false;


    private void Start()
    {
        if (s_hasShownWarning)
        {
            imageCanvasGroup.gameObject.SetActive(false);
            return;
        }

    }

    public void OnConfirmButtonClicked()
    {
        s_hasShownWarning = true;

    }
}
