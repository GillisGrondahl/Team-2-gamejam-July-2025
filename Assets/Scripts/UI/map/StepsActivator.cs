using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StepsActivator : MonoBehaviour
{
    [SerializeField] private GameObject stepContainer; 
    [SerializeField] private float timeBetweenActivations = 0.1f;
    [SerializeField] private bool startRevealOnStart = false;

    private List<GameObject> stepInstances = new List<GameObject>(); 

    private void Start()
    {
        if (stepContainer != null)
        {
            for (int i = 0; i < stepContainer.transform.childCount; i++)
            {
                stepInstances.Add(stepContainer.transform.GetChild(i).gameObject);
            }
        }

        if (startRevealOnStart)
        {
            ShowStepsSequentially();
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StepsActivator))]
    public class StepActivatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StepsActivator stepActivator = (StepsActivator)target;

            if (GUILayout.Button("Activate Steps Sequentially"))
            {
                stepActivator.ShowStepsSequentially();
            }
        }
    }
#endif

    public void HideSteps()
    {
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(false);
        }
    }

    public void ShowStepsSequentially()
    {
        StartCoroutine(ActivateStepsSequentially());
    }

    public void ShowStepsInstantly()
    {
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(true);
        }
    }

    private IEnumerator ActivateStepsSequentially()
    {
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(true);
            yield return new WaitForSeconds(timeBetweenActivations);
        }
    }
}