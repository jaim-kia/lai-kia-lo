using UnityEngine;

public enum ItemCategory { Object, Tape, Charm }

[CreateAssetMenu(menuName = "Game/Items/Object", fileName = "NewObject")]
public class ItemData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    [TextArea(2, 5)] public string description;
    [SerializeField] private int maxStack = 99; // coins 999, ability items 1

    [Tooltip("Tapes and charms are laid out in ascending order")]
    public int sortId;

    public virtual ItemCategory Category => ItemCategory.Object;
    public virtual int MaxStack => maxStack;

    // the asset's file name is the ID, so don't rename assets once you have saves
    public string Id => name;
}