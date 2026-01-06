using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    private void Start()
    {
        ps.Play();
    }
}
