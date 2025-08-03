using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private void OnEnable()
    {
        AudioManager.instance.HandleLevelStop();


    }
}
