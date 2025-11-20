using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Definition", fileName = "NewItem")]
public class ItemDefinition : ScriptableObject
{
    [Header("Id musi być UNIKALNY i zgodny z tym co masz w ekwipunku")]
    public string id = "Stone";          // np. "Stone", "Copper", "Brick", "CopperPlate"

    [Header("Wyświetlana nazwa")]
    public string displayName = "Stone"; // np. "Kamień", "Miedź", "Cegła"

    [Header("Ikona")]
    public Sprite icon;

    [Header("Maksymalna wielkość stacka (opcjonalnie)")]
    public int maxStack = 999;
}
