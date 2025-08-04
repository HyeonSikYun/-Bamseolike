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
        // ��� ������ ��Ȱ��ȭ
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // ���� ������ �����۵��� �ε��� ����Ʈ ����
        List<int> availableItems = new List<int>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].CanBeSelected())
            {
                availableItems.Add(i);
            }
        }

        // ���� ������ �������� 3�� �̸��̸� �Һ� ������ �߰� (�� ������ ��)
        if (availableItems.Count < 3)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item.oneTimeUse && !availableItems.Contains(i))
                {
                    // �Һ� �������� �׻� �ٽ� ���� �����ϵ��� (�� ������ ��)
                    if (item.data.itemType == ItemData.ItemType.Heal)
                    {
                        availableItems.Add(i);
                    }
                }
            }
        }

        // 3�� ���� (������ �������� 3�� �̸��̸� ������ ��ŭ��)
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