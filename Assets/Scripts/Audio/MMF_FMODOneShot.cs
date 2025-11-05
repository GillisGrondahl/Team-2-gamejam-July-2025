using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using FMODUnity;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// When played, this feedback will play an FMOD OneShot event at the specified position with the given parameters.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you play an FMOD OneShot event with optional 3D positioning and parameter control.")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
    [FeedbackPath("Audio/FMOD OneShot")]
    public class MMF_FMODOneShot : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        /// sets the inspector color for this feedback
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        public override string RequiresSetupText { get { return "This feedback requires that an FMOD Event Path be set to be able to work properly. You can set one below."; } }
#endif

        [MMFInspectorGroup("FMOD Event", true, 54, true)]
        /// the FMOD event path to play
        [Tooltip("the FMOD event path to play")]
        public EventReference FMODEventPath;

        [MMFInspectorGroup("Volume", true, 55)]
        /// the volume multiplier for this event
        [Tooltip("the volume multiplier for this event")]
        [Range(0f, 1f)]
        public float Volume = 1f;
        /// whether or not to use the feedback's intensity to control volume
        [Tooltip("whether or not to use the feedback's intensity to control volume")]
        public bool UseIntensityForVolume = false;
        /// the curve to apply to the intensity to volume relationship
        [Tooltip("the curve to apply to the intensity to volume relationship")]
        [MMFCondition("UseIntensityForVolume", true)]
        public AnimationCurve IntensityToVolumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [MMFInspectorGroup("3D Audio", true, 56)]
        /// whether or not to play this sound in 3D space
        [Tooltip("whether or not to play this sound in 3D space")]
        public bool Play3D = false;
        /// the transform to use for 3D positioning (if null, uses the feedback's owner position)
        [Tooltip("the transform to use for 3D positioning (if null, uses the feedback's owner position)")]
        [MMFCondition("Play3D", true)]
        public Transform PositionTransform;

        [MMFInspectorGroup("Parameters", true, 57)]
        /// FMOD parameters to set when playing this event
        [Tooltip("FMOD parameters to set when playing this event")]
        public FMODParameter[] Parameters;

        [System.Serializable]
        public class FMODParameter
        {
            [Tooltip("The name of the FMOD parameter")]
            public string ParameterName;
            [Tooltip("The value to set for this parameter")]
            public float Value;
            [Tooltip("Whether to multiply this value by the feedback's intensity")]
            public bool UseIntensity = false;
            [Tooltip("The curve to apply when using intensity")]
            [MMFCondition("UseIntensity", true)]
            public AnimationCurve IntensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        /// the duration of this feedback is 0 since it's a one-shot
        public override float FeedbackDuration
        {
            get { return 0f; }
            set { /* OneShot events don't have controllable duration */ }
        }

        /// <summary>
        /// On Play we trigger the FMOD OneShot event
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }
            
            //Debug.Log($"Trying to play FMDO event: {FMODEventPath.Path}");
            PlayFMODOneShot(position, feedbacksIntensity);
        }

        /// <summary>
        /// Plays the FMOD OneShot event with the specified parameters
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected virtual void PlayFMODOneShot(Vector3 position, float feedbacksIntensity)
        {
            // Create event instance for parameter control
            var eventInstance = RuntimeManager.CreateInstance(FMODEventPath);

            // Set volume parameter if available
            float finalVolume = UseIntensityForVolume ?
                IntensityToVolumeCurve.Evaluate(feedbacksIntensity) * Volume :
                Volume;

            // Try to set volume parameter (common FMOD parameter name)
            eventInstance.setParameterByName("Volume", finalVolume);

            // Set custom parameters
            if (Parameters != null)
            {
                foreach (var param in Parameters)
                {
                    if (!string.IsNullOrEmpty(param.ParameterName))
                    {
                        float value = param.UseIntensity ?
                            param.IntensityCurve.Evaluate(feedbacksIntensity) * param.Value :
                            param.Value;
                        eventInstance.setParameterByName(param.ParameterName, value);
                    }
                }
            }

            // Set 3D attributes if needed
            if (Play3D)
            {
                Vector3 playPosition = GetPlayPosition(position);
                eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(playPosition));
            }

            // Start the event
            eventInstance.start();

            // Release immediately for one-shot behavior
            eventInstance.release();
        }

        /// <summary>
        /// Gets the position where the sound should play
        /// </summary>
        /// <param name="fallbackPosition"></param>
        /// <returns></returns>
        protected virtual Vector3 GetPlayPosition(Vector3 fallbackPosition)
        {
            if (PositionTransform != null)
            {
                return PositionTransform.position;
            }
            else if (Owner != null)
            {
                return Owner.transform.position;
            }
            else
            {
                return fallbackPosition;
            }
        }

        /// <summary>
        /// On Stop we don't need to do anything for OneShot events
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }
            // OneShot events can't be stopped after they're released
            base.CustomStopFeedback(position, feedbacksIntensity);
        }

        /// <summary>
        /// On restore, we don't need to restore anything for OneShot events
        /// </summary>
        protected override void CustomRestoreInitialValues()
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }
            // Nothing to restore for OneShot events
        }

    }
}