using UnityEngine;

public class EnemyScoreReward : MonoBehaviour
{
    [SerializeField] private int scoreValue = 10;

    public void OnDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.EnemyDied();
        }
    }
}