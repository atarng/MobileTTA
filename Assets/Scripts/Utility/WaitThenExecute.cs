using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitThenExecute : MonoBehaviour {
    public static void CreateWaitForSecondsThanExecute(float seconds, Action toExecute) {
        GameObject go = new GameObject();
        WaitThenExecute wte = go.AddComponent<WaitThenExecute>();
        wte.StartCoroutine(wte.WaitForSecondsThenExecute(seconds, toExecute));
    }
    IEnumerator WaitForSecondsThenExecute(float timeToWait, Action toExecute) {
        yield return new WaitForSeconds(timeToWait);
        if(toExecute != null) {
            toExecute();
        }
    }
}
