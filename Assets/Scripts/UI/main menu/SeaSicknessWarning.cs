using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using VContainer;

public class SeaSicknessWarning : MonoBehaviour
{
    [SerializeField] private CanvasGroup imageCanvasGroup;
    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private MMF_Player fdbkClickToSkip;

    // Survives scene reloads within the same app session
    private static bool s_hasShownWarning = false;

    private IInputService _input;

    [Inject]
    public void Construct(IInputService input)
    {
        _input = input;
    }



    private void Start()
    {
        if (s_hasShownWarning)
        {
            imageCanvasGroup.alpha = 0f;
            textCanvasGroup.alpha = 0f;
            imageCanvasGroup.gameObject.SetActive(false);
            return;
        }

        StartCoroutine(ShowThenFade());
    }

    private IEnumerator ShowThenFade()
    {
        s_hasShownWarning = true;
        imageCanvasGroup.alpha = 1f;
        textCanvasGroup.alpha = 1f;

        // Wait for display duration OR a click
        float elapsed = 0f;
        while (elapsed < displayDuration && (_input == null || !_input.IsInteractPressed))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Play click feedback only if skipped early
        if (_input != null && _input.IsInteractPressed)
            fdbkClickToSkip?.PlayFeedbacks();

        // Fade out text first
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            textCanvasGroup.alpha = 1f - (t * t);
            yield return null;
        }
        textCanvasGroup.alpha = 0f;

        // Then fade out the image
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            imageCanvasGroup.alpha = 1f - (t * t);
            yield return null;
        }

        imageCanvasGroup.alpha = 0f;
        imageCanvasGroup.gameObject.SetActive(false);
    }
}
