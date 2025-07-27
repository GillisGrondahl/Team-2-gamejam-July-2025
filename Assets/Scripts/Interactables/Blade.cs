using System.Collections;
using UnityEngine;

public class Blade : MonoBehaviour
{

    private bool _isKnifing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Ingredient ingredient) && !_isKnifing)
        {
            _isKnifing = true;
            Cutter.Cut(ingredient.gameObject, transform.position, transform.up);
            StartCoroutine("WaitForNextSlice");
            // _cutIngredient = ingredient;
        }
    }
    private IEnumerator WaitForNextSlice()
    {
        yield return new WaitForSeconds(1f);
        _isKnifing = false;
        //_cutIngredient = null;
    }

    //void OnDrawGizmos()
    //{
    //    if (transform == null) return;

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawRay(transform.position, transform.forward); // blade forward?
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawRay(transform.position, transform.up); // blade up?
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawRay(transform.position, transform.right); // blade right?
    //}
}
