using UnityEngine;

public class Weapon : MonoBehaviour
{
    float timer;
    Player player;
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;

    private void Awake()
    {
        player = GameManager.instance.player;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            case 6: // Magic
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    FireMagic();
                }
                break;
            default:
                timer += Time.deltaTime;
                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }

        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(10, 1);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count += count;
        if (id == 0)
            Batch();
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
        UpgradeCompanionWeapons();
    }

    public void Init(ItemData data, Transform owner = null)
    {
        Transform parent = owner != null ? owner : GameManager.instance.player.transform;

        name = "Weapon" + data.itemId;
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        id = data.itemId;
        damage = data.baseDamage;
        count = data.baseCount;

        for (int i = 0; i < GameManager.instance.pool.prefabs.Length; i++)
        {
            if (data.projectile == GameManager.instance.pool.prefabs[i])
            {
                prefabId = i;
                break;
            }
        }

        switch (id)
        {
            case 0:
                speed = 150;
                Batch();
                break;
            case 6:
                speed = 3f; // Magic 발사 간격
                break;
            default:
                speed = 0.5f;
                break;
        }

        parent.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    void Batch()
    {
        for (int index = 0; index < count; index++)
        {
            Transform bullet;
            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            Vector3 rotVec = Vector3.forward * 360 * index / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 0.9f, Space.World);
            bullet.GetComponent<Bullet>().Init(damage, -1, Vector3.zero);
        }
    }

    void Fire()
    {
        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);
    }

    void FireMagic()
    {
        int bulletCount = 8;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = 360f / bulletCount * i;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.up;

            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.position = transform.position;
            bullet.rotation = Quaternion.Euler(0, 0, angle);
            bullet.GetComponent<Bullet>().Init(damage, count, dir);
        }
    }

    void UpgradeCompanionWeapons()
    {
        if (id == 0) return;

        CompanionAI[] companions = FindObjectsOfType<CompanionAI>();
        foreach (CompanionAI companion in companions)
        {
            if (companion != null)
            {
                companion.UpgradeCompanionWeapon(0.2f, 0.9f);
            }
        }
    }
}
