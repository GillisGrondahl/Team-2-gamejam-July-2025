using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    public void SetData(LeaderboardEntry entry)
    {
        rankText.text = entry.Rank.ToString();
        nameText.text = entry.Timestamp.ToString("dd.MM.yyyy hh:mm");
        scoreText.text = entry.Score.ToString();
    }
}
