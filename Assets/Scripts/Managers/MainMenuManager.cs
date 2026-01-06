using UnityEngine;
using VContainer;

public class MainMenuManager : MonoBehaviour
{

    IGameStateService _state;

    StateMask MainMenuMask = StateMask.CursorVisible | StateMask.CursorUnlocked;

    [Inject]
    public void Construct(IGameStateService stateService)
    {
        _state = stateService;
    }

    private void Start()
    {
        _state.SetGameState(GameState.MainMenu);
    }



}
