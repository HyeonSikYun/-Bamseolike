using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;
    public bool oneTimeUse = false;
    public bool isUsed = false;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;

    private void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;
        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];
        textName = texts[1];
        textDesc = texts[2];
        textName.text = data.itemName;

        if (data.itemType == ItemData.ItemType.Friend || data.itemType == ItemData.ItemType.Heal)
        {
            oneTimeUse = true;
        }
    }

    private void OnEnable()
    {
        textLevel.text = "Lv." + (level + 1);
        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
            case ItemData.ItemType.Magic:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
                break;
            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
                break;
            default:
                textDesc.text = data.itemDesc; // 매개변수 없이 처리
                break;
        }
    }

    public void OnClick()
    {
        if (isUsed) return;

        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
            case ItemData.ItemType.Magic:
                if (level == 0)
                {
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else
                {
                    float nextDamage = data.baseDamage + data.baseDamage * data.damages[level];
                    int nextCount = data.counts[level];
                    weapon.LevelUp(nextDamage, nextCount);
                }
                level++;
                break;

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }
                level++;
                break;

            case ItemData.ItemType.Heal:
                GameManager.instance.health = GameManager.instance.maxHealth;
                break;

            case ItemData.ItemType.Friend:
                Vector3 spawnPos = GameManager.instance.player.transform.position + Random.insideUnitSphere * 2f;
                spawnPos.y = 0;
                GameObject companion = Instantiate(data.summonPrefab, spawnPos, Quaternion.identity);

                CompanionAI companionAI = companion.GetComponent<CompanionAI>();
                if (companionAI != null)
                {
                    companionAI.Initialize(GameManager.instance.player.transform);
                }

                level++;
                if (level >= 5)
                {
                    isUsed = true;
                    GetComponent<Button>().interactable = false;
                }
                break;
        }

        if (!oneTimeUse && level == data.damages.Length)
        {
            GetComponent<Button>().interactable = false;
        }
    }

    public bool CanBeSelected()
    {
        if (oneTimeUse && isUsed) return false;
        if (!oneTimeUse && level >= data.damages.Length) return false;
        return true;
    }
}
