using System;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public string itemID;
    public int levelDo;     // +0 -> +10
    public int quantity;

    [NonSerialized]
    public ItemData itemData;

    private const float FORGE_RATE = 0.1f; // +10% mỗi cấp

    public int GetHP()
        => itemData == null ? 0 : Mathf.RoundToInt(itemData.baseHP * (1 + levelDo * FORGE_RATE));

    public int GetAttack()
        => itemData == null ? 0 : Mathf.RoundToInt(itemData.baseAttack * (1 + levelDo * FORGE_RATE));

    public int GetPhongThu()
        => itemData == null ? 0 : Mathf.RoundToInt(itemData.basePhongThu * (1 + levelDo * FORGE_RATE));

    public int GetNeTranh()
        => itemData == null ? 0 : Mathf.RoundToInt(itemData.baseNeTranh * (1 + levelDo * FORGE_RATE));

    public int GetTocDo()
        => itemData == null ? 0 : Mathf.RoundToInt(itemData.baseTocDo * (1 + levelDo * FORGE_RATE));
}
