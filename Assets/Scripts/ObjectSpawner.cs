using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject TriggerCube;
    public GameObject TriggerDoor;
    public GameObject TriggerInvestigate;
    public GameObject TriggerUse;
    public GameObject TriggerTalk;
    public GameObject TriggerAction;
    public GameObject TriggerRndWaypoint;
    public GameObject TriggerPlaythrough;
    public GameObject TriggerHint;
    public GameObject TriggerImage;
    public GameObject TriggerImageUse;
    public GameObject TriggerAudioInteractable;
    public GameObject TriggerAudioWaypoint;
    public GameObject TriggerAudio;
    public GameObject TriggerChecklistRoot;
    public GameObject TriggerCheckbox;
    public GameObject TriggerTextButton;
    public GameObject TriggerQuiz;
    public GameObject SpawnPoint;
    public GameObject InteractionSphere;
    public Transform XRRig;
    
    private GameObject lastSpawnedObject;
    
    public void spawnDoorTrigger()
    {
        spawnTrigger(TriggerDoor);
    }
    
    public void spawnInvestigateTrigger()
    {
        spawnTrigger(TriggerInvestigate);
    }
    
    public void spawnUseTrigger()
    {
        spawnTrigger(TriggerUse);
    }
    
    public void spawnTalkTrigger()
    {
        spawnTrigger(TriggerTalk);
    }
    
    public void spawnRndWaypointTrigger()
    {
        spawnTrigger(TriggerRndWaypoint);
    }
    
    public void spawnCubeTrigger()
    {
        spawnTrigger(TriggerCube);
    }

    public void spawnActionTrigger()
    {
        spawnTrigger(TriggerAction);
    }
    
    public void spawnHintTrigger()
    {
        spawnTrigger(TriggerHint);
    }
    
    public void spawnPlaythroughTrigger()
    {
        spawnTrigger(TriggerPlaythrough);
    }
    
    public void spawnImageTrigger()
    {
        spawnTrigger(TriggerImage);
    }
    
    public void spawnImageUseTrigger()
    {
        spawnTrigger(TriggerImageUse);
    }
    
    public void spawnAudioInteractableTrigger()
    {
        spawnTrigger(TriggerAudioInteractable);
    }
    
    public void spawnAudioWaypointTrigger()
    {
        spawnTrigger(TriggerAudioWaypoint);
    }
    
    public void spawnAudioTrigger()
    {
        spawnTrigger(TriggerAudio);
    }
    
    public void spawnChecklistRootTrigger()
    {
        spawnTrigger(TriggerChecklistRoot);
    }
    
    public void spawnCheckboxTrigger()
    {
        spawnTrigger(TriggerCheckbox);
    }
    
    public void spawnTextButtonTrigger()
    {
        spawnTrigger(TriggerTextButton);
    }
    
    public void spawnQuizTrigger()
    {
        spawnTrigger(TriggerQuiz);
    }
    
    private void spawnTrigger(GameObject triggerToSpawn)
    {
        clearSpawner();
        lastSpawnedObject = Instantiate(triggerToSpawn, SpawnPoint.transform.position, 
            Quaternion.Euler(new Vector3(1,XRRig.rotation.eulerAngles.y,1)));
        lastSpawnedObject.transform.parent = SpawnPoint.transform;
        lastSpawnedObject.GetComponent<Base360Interactable>().InteractionSphere = InteractionSphere;
        Canvas[] canvas = lastSpawnedObject.GetComponentsInChildren<Canvas>(true);
        foreach (var can in canvas)
        {
            can.worldCamera = Camera.main;
        }
    }

    internal void clearSpawner()
    {
        if (lastSpawnedObject != null &&
            SpawnPoint.GetComponent<BoxCollider>().bounds.Contains(lastSpawnedObject.transform.position))
        {
            Destroy(lastSpawnedObject);
        }
    }
}
