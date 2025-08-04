using UnityEngine;

public class Spawner : MonoBehaviour
{
    float timer;
    int level;
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public BossData bossData;
    public float levelTime;
    private bool bossSpawned = false; // ������ �̹� ��ȯ�Ǿ����� Ȯ���ϴ� �÷���

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

        // ���� 5(�ε��� 4)���� ���� ��ȯ
        if (level >= 4 && !bossSpawned)
        {
            SpawnBoss();
            bossSpawned = true;
        }

        // �Ϲ� ���ʹ� ��� ��ȯ
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
        // ���� 5���� ��ȯ
        for (int i = 0; i < 5; i++)
        {
            GameObject boss = GameManager.instance.pool.Get(bossData.enemyType);
            boss.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

            // ���� ������Ʈ���� Boss ������Ʈ�� ���
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