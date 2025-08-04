using UnityEngine;

public class Spawner : MonoBehaviour
{
    float timer;
    int level;
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public BossData bossData;
    public float levelTime;
    private bool bossSpawned = false; // 보스가 이미 소환되었는지 확인하는 플래그

    private void Start()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        // 레벨 5(인덱스 4)에서 보스 소환
        if (level >= 4 && !bossSpawned)
        {
            SpawnBoss();
            bossSpawned = true;
        }

        // 일반 몬스터는 계속 소환
        if (timer > spawnData[level].spawnTime)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(level);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponentInChildren<Enemy>().Init(spawnData[level]);
    }

    void SpawnBoss()
    {
        // 보스 5마리 소환
        for (int i = 0; i < 5; i++)
        {
            GameObject boss = GameManager.instance.pool.Get(bossData.enemyType);
            boss.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

            // 보스 오브젝트에는 Boss 컴포넌트를 사용
            Boss bossComponent = boss.GetComponentInChildren<Boss>();
            if (bossComponent != null)
            {
                bossComponent.Init(bossData);
            }
        }
    }
}

[System.Serializable]
public class SpawnData
{
    public int enemyType;
    public float spawnTime;
    public int health;
    public float speed;
}

[System.Serializable]
public class BossData
{
    public int enemyType;
    public int health;
    public float speed;
}