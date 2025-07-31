using System.Collections;
using UnityEngine;

[RequireComponent(typeof(VelocityTracker))]
public class SaltShaker : Tool
{
    [SerializeField] Transform saltParticles;
    [SerializeField] float velocityThreshold = 2.0f;
    [SerializeField] float shakeCooldown = 1.0f;
    [SerializeField] bool isOnCooldown = false;
    [SerializeField] Transform saltSpawnPoint;


    private VelocityTracker _velocityTracker;


    protected override void Awake()
    {
        base.Awake();
        _velocityTracker = GetComponent<VelocityTracker>();
    }

    private void LateUpdate()
    {
        if (_velocityTracker.Velocity.magnitude > velocityThreshold && _velocityTracker.IsMovingDownward)
        {
            Use();
        }
    }


    public void Use()
    {
        if (saltParticles == null || isOnCooldown) return;
        Instantiate(saltParticles, transform.position, Quaternion.Euler(90f, 0f, 0f));

        isOnCooldown = true;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(shakeCooldown);
        isOnCooldown = false;
    }
}
