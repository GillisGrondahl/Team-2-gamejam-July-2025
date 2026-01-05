using System.Collections.Generic;
using UnityEngine;

public sealed class InteractableGroup : MonoBehaviour
{
    [SerializeField] private List<Interactable> members = new();

    private int _hoverRefs;

    public void Register(Interactable interactable)
    {
        if (interactable != null && !members.Contains(interactable))
            members.Add(interactable);
    }

    public void Unregister(Interactable interactable)
    {
        members.Remove(interactable);
    }

    // Called by ANY member on hover start
    public void GroupHoverStart(Interactor _)
    {
        _hoverRefs++;
        if (_hoverRefs == 1)
            SetOutlined(true);
    }

    // Called by ANY member on hover end
    public void GroupHoverEnd(Interactor _)
    {
        _hoverRefs = Mathf.Max(0, _hoverRefs - 1);
        if (_hoverRefs == 0)
            SetOutlined(false);
    }

    private void SetOutlined(bool outlined)
    {
        for (int i = 0; i < members.Count; i++)
        {
            var it = members[i];
            if (it == null) continue;

            if (it.TryGetComponent<LayerOutlinePresenter>(out var presenter))
                presenter.SetOutlined(outlined);
        }
    }
}
