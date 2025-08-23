using UnityEngine;

[CreateAssetMenu(fileName = "LevelDifficultySO", menuName = "ScriptableObjects/LevelDifficultySO")]
public class LevelDifficultySO : ScriptableObject
{
    public int timeToComplete1Star = 240;
    public int timeToComplete2Star = 180;
    public int timeToComplete3Star = 120;
    public int timetoComplete4Star = 90;
    public int timeToComplete5Star = 60;
}
