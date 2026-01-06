using UnityEngine;

public interface IOutlinePresenter
{
    void SetOutlined(bool outlined);

}
public class LayerOutlinePresenter : MonoBehaviour, IOutlinePresenter
{
    [field: SerializeField] public LayerMask OriginalLayer { get; set; }
    [field: SerializeField] public LayerMask OutlineLayer { get; set; }
    [field: SerializeField] bool ApplyToChildren { get; set; }

    public void SetOutlined(bool outlined)
    {
        int layer = GetLayerFromMask(outlined ? OutlineLayer.value : OriginalLayer.value);
        if (ApplyToChildren) SetLayerRecursively(gameObject, layer);
        else gameObject.layer = layer;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        var t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    private static int GetLayerFromMask(int mask)
    {
        int layer = 0;
        while ((mask >>= 1) != 0) layer++;
        return layer;
    }
}
