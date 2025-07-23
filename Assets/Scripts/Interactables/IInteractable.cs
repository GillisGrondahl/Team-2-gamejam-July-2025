using UnityEngine;

public interface IInteractable
{
    public LayerMask OutlineLayer { get; }
    public LayerMask OriginalLayer { get; }
    public IInteractable Use(Transform target);
    public void Release();
    public void ShowOutline();
    public void HideOutline();

    public int GetLayerFromMask(int mask)
    {
        int layer = 0;
        while ((mask >>= 1) != 0)
            layer++;
        return layer;
    }
}
