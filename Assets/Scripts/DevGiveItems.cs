using UnityEngine;

public class DevGiveItems : MonoBehaviour
{
    public Inventory inv;
    void Start()
    {
        inv.AddResource("Furnace", 3);   // 3 szt. pieców do budowy
        inv.AddResource("Stone", 10);
        inv.AddResource("Miner", 3);
        inv.AddResource("Belt", 30);
        inv.AddResource("Chest", 3);
        inv.AddResource("CopperPlate", 3);
        inv.AddResource("Brick", 3);
    }
}
