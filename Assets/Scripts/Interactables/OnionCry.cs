using MoreMountains.Feedbacks;
using UnityEngine;

public class OnionCry : MonoBehaviour
{
    [SerializeField] private MMF_Player _MMFOnionCry;

    private bool _isCut = false;

    public void PlayCry()
    {
        if (_isCut == false)
        {
            _MMFOnionCry.PlayFeedbacks();
            _isCut = true;
        }
    }


}
