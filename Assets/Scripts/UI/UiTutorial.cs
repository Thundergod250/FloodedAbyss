using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiTutorial : MonoBehaviour
{
    [Header("Tutorial Information")]
    [SerializeField] private List<GameObject> tutorialList = new List<GameObject>();
    private void Start()
    {
        tutorialList[0].SetActive(true);
        StartCoroutine(CloseFirstTutorial());
    }

    public void OpenUI()
    {
        ObjectiveManager om = ObjectiveManager.Instance;

        if (om == null)
        {
            Debug.LogError("[UiTutorial] ObjectiveManager.Instance is null.");
            return;
        }

        tutorialList[0].SetActive(false);

        for (int i = 1; i < tutorialList.Count; i++)
        {
            if (tutorialList[i] != null)
                tutorialList[i].SetActive(false);
        }

        if (om.CQI == 1)
        {
            tutorialList[1].SetActive(true);
        }
        else if (om.CQI == 3)
        {
            tutorialList[2].SetActive(true);
        }
    }

    private IEnumerator CloseFirstTutorial()
    {
        yield return new WaitForSeconds(30f);

        if (tutorialList[0] != null)
            tutorialList[0].SetActive(false);
    }
}


 