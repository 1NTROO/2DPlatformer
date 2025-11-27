using UnityEngine;
using System.Collections;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void TriggerShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulse();
    }
}
