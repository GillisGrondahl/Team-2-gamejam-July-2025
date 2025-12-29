using MoreMountains.Feedbacks;
using UnityEngine;

[AddComponentMenu("More Mountains/Feedbacks/MMF_OpenURL")]
[FeedbackPath("Application/Open URL")]
public class MMF_OpenURL : MMF_Feedback
{
    /// Sets the color of this feedback in the inspector
    public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }

    /// a static bool used to disable all feedbacks of this type at once
    public static bool FeedbackTypeAuthorized = true;

    [MMFInspectorGroup("URL Settings", true, 10)]
    [Tooltip("The URL to open when this feedback plays")]
    public string URL;

    [Tooltip("Cooldown duration in seconds to prevent rapid successive URL opens")]
    public float Cooldown = 2.0f;

    private float _lastOpenTime = -999f;

    /// <summary>
    /// On Play, opens the specified URL if cooldown has elapsed
    /// </summary>
    /// <param name="position"></param>
    /// <param name="feedbacksIntensity"></param>
    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        if (!Active || !FeedbackTypeAuthorized)
        {
            return;
        }

        // Check if cooldown has elapsed
        if (Time.time >= _lastOpenTime + Cooldown)
        {
            _lastOpenTime = Time.time;
            Application.OpenURL(URL);
        }
    }
}
