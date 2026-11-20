using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Charm", fileName = "NewCharm")]
public class CharmData : ItemData
{
    public override ItemCategory Category => ItemCategory.Charm;
    public override int MaxStack => 1;
}