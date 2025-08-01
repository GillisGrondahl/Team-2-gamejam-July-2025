using MoreMountains.Feedbacks;
using UnityEngine;

public class OnionCry : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFOnionCry;

    public void PlayCry()
    {
        _MMFOnionCry.PlayFeedbacks();
    }


}
