using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

public class StepsActivator : MonoBehaviour
{
    [SerializeField] private GameObject stepContainer; 
    [SerializeField] private float timeBetweenActivations = 0.1f;
    [SerializeField] private bool startRevealOnStart = false;
    [SerializeField] private float waitForFadeIn = 2.5f;

    private List<GameObject> stepInstances = new List<GameObject>(); 

    private void Awake()
    {
        if (stepContainer != null)
        {
            for (int i = 0; i < stepContainer.transform.childCount; i++)
            {
                stepInstances.Add(stepContainer.transform.GetChild(i).gameObject);
            }
        }
    }

    private void Start()
    {
        if (startRevealOnStart)
        {
            ShowStepsSequentially();
        }
    }

    public void HideSteps()
    {
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(false);
        }
    }

    [ContextMenu("Activate Steps Sequentially")]
    public void ShowStepsSequentially()
    {
        StartCoroutine(ActivateStepsSequentially());
    }

    [ContextMenu("Activate Steps Instantly")]
    public void ShowStepsInstantly()
    {
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(true);
        }
    }

    private IEnumerator ActivateStepsSequentially()
    {
        yield return new WaitForSeconds(waitForFadeIn);
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(true);
            yield return new WaitForSeconds(timeBetweenActivations);
        }
    }
}