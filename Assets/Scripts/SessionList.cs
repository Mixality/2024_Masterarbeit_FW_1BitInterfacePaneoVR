using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Network;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.UI.WebService;
using UnityEngine.Video;

public class SessionList : MonoBehaviour
{
    private static string SURE_TO_DELETE = "Sind Sie sicher, dass sie das Projekt löschen wollen?";

    private string selectedProject;
    
    public Image listGrid;

    private UIController UIController;

    private Lobby selectedLobby;
    
    public Button templateButton;
    
    private LobbyManager lobbyManager;
    
    private NetworkController networkController;
    
    private void Start()
    {
        UIController = FindObjectOfType<UIController>();
        if (!UIController)
        {
            Debug.LogError("Error in Project List: Couldn't find UIController in Scene");
        }
        
        networkController = FindObjectOfType<NetworkController>();
        if (!networkController)
        {
            Debug.Log("No NetworkController found in scene!");
        }
        
        lobbyManager = FindObjectOfType<LobbyManager>();
    }

    public void LookForSessions()
    {
        lobbyManager.LookForSessions(this);
    }

    internal void QickFindSession()
    {
        
    }
    
    public void JoinSelectedSession()
    {
        networkController.JoinProject(selectedLobby);
    }
    
    internal void ReloadSessions(List<Lobby> lobbies)
    {
        foreach (Transform child in listGrid.transform)
        {
            Destroy(child.gameObject);
        }
        
        Button newEntry;
        
        foreach (var lobby in lobbies)
        {
            newEntry = Instantiate(templateButton);
            newEntry.onClick.AddListener(delegate
            {
                setSelectedLobby(lobby);
            });
            newEntry.GetComponentInChildren<Text>().text = lobby.Name;
            newEntry.transform.SetParent(listGrid.transform, false);
        }
    }

    private void setSelectedLobby(Lobby lobby)
    {
        selectedLobby = lobby;
    }
}
