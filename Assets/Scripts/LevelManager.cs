using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Enemy Tracking")]
    [SerializeField] private int enemyCount;

    [Header("Boss Door")]
    [SerializeField] private BossDoor bossDoor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterEnemy()
    {
        enemyCount++;
    }

    public void EnemyDied()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            CheckRoomClear();
        }
    }

    private void CheckRoomClear()
    {
        if (bossDoor != null)
        {
            bossDoor.Open();
        }
    }
}