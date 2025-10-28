using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class ResourceNode : MonoBehaviour
{
    [Header("Ustawienia surowca")]
    public string resourceName = "Stone";
    public int amountPerClick = 1;

    [Header("Podświetlenie")]
    public Color highlightColor = Color.yellow;

    private Renderer _renderer;
    private Color _baseColor;
    private bool _isHighlighted;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _baseColor = _renderer.material.color;
    }

    public void SetHighlight(bool on)
    {
        if (_isHighlighted == on) return;
        _isHighlighted = on;
        _renderer.material.color = on ? highlightColor : _baseColor;
    }

    public void Collect(Inventory inventory)
    {
        if (inventory == null) return;
        inventory.AddResource(resourceName, amountPerClick);
        // Złoże NIE znika — nic nie usuwamy.
    }

    private void OnDisable()
    {
        // Na wszelki wypadek przywróć kolor
        if (_renderer != null) _renderer.material.color = _baseColor;
    }
}
