using UnityEngine;

public struct WaveConfiguration
{
    private float enemySpawnInterval;
    private int enemiesToSpawn;
    private float enemyHealthMultiplier;
    private float enemyDamageMultiplier;

    public WaveConfiguration(float enemySpawnInterval, int enemiesToSpawn, float enemyHealthMultiplier, float enemyDamageMultiplier)
    {
        this.enemiesToSpawn = enemiesToSpawn;
        this.enemySpawnInterval = enemySpawnInterval;
        this.enemyHealthMultiplier = enemyHealthMultiplier;
        this.enemyDamageMultiplier = enemyDamageMultiplier;
    }

    // Read Only Properties
    public float EnemySpawnInterval => enemySpawnInterval;
    public int EnemiesToSpawn => enemiesToSpawn;
    public float EnemyHealthMultiplier => enemyHealthMultiplier;
    public float EnemyDamageMultiplier => enemyDamageMultiplier;

}
