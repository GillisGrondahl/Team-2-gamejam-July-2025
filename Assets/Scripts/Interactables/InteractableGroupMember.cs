using UnityEngine;

[RequireComponent(typeof(Interactable))]
public sealed class InteractableGroupMember : MonoBehaviour
{
    [SerializeField] private InteractableGroup group;

    private Interactable _interactable;
    private bool _registered;

    public InteractableGroup Group => group;

    private void Awake()
    {
        _interactable = GetComponent<Interactable>();
    }

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        Unregister();
    }

    public void SetGroup(InteractableGroup g)
    {
        if (group == g) return;

        // If we were registered to a previous group, detach first
        Unregister();

        group = g;
        TryRegister();
    }

    private void TryRegister()
    {
        if (_registered) return;
        if (!isActiveAndEnabled) return;
        if (group == null) return;

        group.Register(_interactable);

        _interactable.OnHover.AddListener(group.GroupHoverStart);
        _interactable.OnStopHover.AddListener(group.GroupHoverEnd);

        _registered = true;
    }

    private void Unregister()
    {
        if (!_registered) return;

        if (group != null)
        {
            _interactable.OnHover.RemoveListener(group.GroupHoverStart);
            _interactable.OnStopHover.RemoveListener(group.GroupHoverEnd);
            group.Unregister(_interactable);
        }

        _registered = false;
    }
}
