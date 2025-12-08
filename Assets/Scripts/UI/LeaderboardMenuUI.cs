using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LeaderboardMenuUI : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private LeaderboardEntryUI entryPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private int entriesToShow = 10;


    ILeaderboardService _leaderboardService;

    [Inject]
    public void Constructor(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    private async void OnEnable()
    {
        exitButton.onClick.AddListener(OnExitButtonClicked);
        await RefreshAsync();
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(OnExitButtonClicked);
        ClearEntries();
    }

    private void OnExitButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void ClearEntries()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private async Task RefreshAsync()
    {
        ClearEntries();

        var entries = await _leaderboardService.GetTopEntriesAsync(entriesToShow);


        foreach (var entry in entries)
        {
            var entryView = Instantiate(entryPrefab, content);
            entryView.SetData(entry);
        }

    }


}

