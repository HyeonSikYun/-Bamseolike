using UnityEngine;
using System.Collections.Generic;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
    }

    public void Select(int index)
    {
        items[index].OnClick();
    }

    void Next()
    {
        // 모든 아이템 비활성화
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 선택 가능한 아이템들의 인덱스 리스트 생성
        List<int> availableItems = new List<int>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].CanBeSelected())
            {
                availableItems.Add(i);
            }
        }

        // 선택 가능한 아이템이 3개 미만이면 소비 아이템 추가 (힐 아이템 등)
        if (availableItems.Count < 3)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item.oneTimeUse && !availableItems.Contains(i))
                {
                    // 소비 아이템은 항상 다시 선택 가능하도록 (힐 아이템 등)
                    if (item.data.itemType == ItemData.ItemType.Heal)
                    {
                        availableItems.Add(i);
                    }
                }
            }
        }

        // 3개 선택 (가능한 아이템이 3개 미만이면 가능한 만큼만)
        int selectCount = Mathf.Min(3, availableItems.Count);
        List<int> selectedIndices = new List<int>();

        for (int i = 0; i < selectCount; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = availableItems[Random.Range(0, availableItems.Count)];
            }
            while (selectedIndices.Contains(randomIndex));

            selectedIndices.Add(randomIndex);
            items[randomIndex].gameObject.SetActive(true);
        }
    }
}