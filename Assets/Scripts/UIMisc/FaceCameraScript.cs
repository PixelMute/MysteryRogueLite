using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes the attached object face towards the camera on load
public class FaceCameraScript : MonoBehaviour
{
    void Awake()
    {
        transform.LookAt(transform.position + BattleManager.mainCamera.transform.forward);
    }
}
