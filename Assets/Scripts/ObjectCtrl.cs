using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCtrl : MonoBehaviour
{
    void OffActive() {
        gameObject.SetActive(false);
    }

    public void TrapDoorDestroy() {
        Invoke("OffActive", 0.25f);
    }
    
}
