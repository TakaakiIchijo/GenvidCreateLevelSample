using System;
using System.Collections.Generic;
using System.Linq;
using GenvidSDKCSharp;
using UnityEngine;

public class PillarCreator : MonoBehaviour
{
    public GameObject pillarPrefab;
    private List<GameObject> pillarList = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            var pillarObject = Instantiate(pillarPrefab, new Vector3(0f, 2.5f, 0f), Quaternion.identity);
            pillarObject.SetActive(false);
            pillarList.Add(pillarObject);
        }
    }

    public void SetStageData(string eventID, GenvidSDK.EventResult[] results, int numResult, 
        IntPtr userData)
    {
        var selectedPlayerNumberStr = results[0].key.fields[0];
        var value = results[0].values[0].value;
        
        Debug.Log("selectedPlayerNumberStr " +selectedPlayerNumberStr +" value " +value);
        
        var strArray = selectedPlayerNumberStr.Split(',').ToList();

        int[] intArray = strArray.Select(n => Convert.ToInt32(n)).ToArray();

        for (int index = 0; index < intArray.Length; index++)
        {
            if(intArray[index] == 0) continue;

            var posX = 12 - ((index % 5) * 3);
            var posY = (index / 5) * 3;

            var pillarObject = pillarList.FirstOrDefault(p => p.activeSelf == false);
            pillarObject.transform.SetPositionAndRotation(new Vector3(posX, 2.5f, posY), Quaternion.identity);
            pillarObject.SetActive(true);
        }
    }
}
