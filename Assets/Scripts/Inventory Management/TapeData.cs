using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Tape", fileName = "NewTape")]
public class TapeData : ItemData
{
    [Header("Tape")]
    public TextAsset inkJSON;
    public string speakerLabel = "Cassette Tape";

    public override ItemCategory Category => ItemCategory.Tape;
    public override int MaxStack => 1;
}