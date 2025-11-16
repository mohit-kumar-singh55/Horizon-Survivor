using UnityEngine;

[CreateAssetMenu(fileName = "NewDifficultySettings", menuName = "Game/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    public float viewRadius;
    public float detectionTime;
    public float losePlayerTime;
    public float dayDuration;
    public float enemyChaseSpeed;
    [Range(1, 5)] public int kicksToDie;
    public int totalEnemies;
}