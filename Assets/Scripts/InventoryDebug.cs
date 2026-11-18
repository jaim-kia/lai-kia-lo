using UnityEngine;

public class InventoryDebug : MonoBehaviour
{
    [SerializeField] private ItemData[] testItems;

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
            foreach (var item in testItems)
                Inventory.Instance.Add(item, 3);
    }
}