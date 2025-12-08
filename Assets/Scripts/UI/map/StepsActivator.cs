using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
                var stepElement = stepContainer.transform.GetChild(i).gameObject;
                stepInstances.Add(stepElement);
                stepElement.SetActive(false);
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
        ActivateSteps().Forget();
    }

    [ContextMenu("Activate Steps Instantly")]
    public void ShowStepsInstantly()
    {
        foreach (GameObject step in stepInstances)
        {
            step.SetActive(true);
        }
    }

    private async UniTaskVoid ActivateSteps()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitForFadeIn));
        foreach (GameObject step in stepInstances)
        {
            if (step == null) return;
            step.SetActive(true);
            await UniTask.Delay(System.TimeSpan.FromSeconds(timeBetweenActivations));
        }
    }
}