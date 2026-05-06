using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Dungeon/Level Data", order = 0)]
public class LevelData : ScriptableObject
{
    [Header("Level Identity")]
    public string levelName = "Level 1";

    [Header("Grid Settings")]
    public int gridWidth = 30;
    public int gridHeight = 30;

    [Header("Random Walk Settings")]
    public int walkSteps = 200;

    [Header("Rooms")]
    public int roomCount = 5;
    public int minRoomSize = 4;
    public int maxRoomSize = 7;

    [Header("Corridor Width")]
    public bool widenCorridors = true;

    [Header("Walls")]
    public float wallHeight = 3f;

    [Header("Enemies")]
    public int enemyCount = 5;
    public int safeRadius = 5;
}