using UnityEngine;
using System.Collections;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine.UI.SaveSystem;
using WebService;
using NativeWebSocket;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket ws;
    private ProjectController projectController;
    private EditModeController editModeController;
    private ProjectManager ProjectManager;

    private bool authoringMode;
    private bool isConnected;
    
    void Start()
    {
        if (PlayerPrefs.GetInt("loggedIn") >= 1)
        {
            ConnectToServer();
        }
    }

    internal void InitEditProjectConnection(ProjectController pController, EditModeController eController)
    {
        ProjectManager = null;
        projectController = pController;
        editModeController = eController;
        authoringMode = true;
        sendActiveProjectToWsServer(projectController.activeProject.projectSrc);
    }
    
    internal void InitMainMenuConnection(ProjectManager projectManager)
    {
        projectController = null;
        editModeController = null;
        ProjectManager = projectManager;
        authoringMode = false;
        sendCloseProjectToWsServer();
    }
    
    public void TryConnectToServer()
    {
        if (!isConnected)
        {
            ConnectToServer();
        }
    }

    private void ServerEvents(byte[] data)
    {
        string message = System.Text.Encoding.UTF8.GetString(data);
        switch (message)
        {
            case "updateprojects":
                if (ProjectManager != null)
                {
                    ThreadSafeInvoker.ExecuteInMainThread(() => ProjectManager.OnProjectsChanged.Invoke());
                }
                break;
            case "updateAndReloadActiveProject":
                if (editModeController != null)
                {
                    ThreadSafeInvoker.ExecuteInMainThread(() =>
                        projectController.saveController.updateProjectFromRemoteWithReload());
                }
                break;
            default:
                Debug.Log("Unknown call: " + message);
                break;
        }
    }

    private void sendActiveProjectToWsServer(string projectSrc)
    {
        var message = new {call = "openedProject", src = projectSrc };
        SendMessageToServer(JsonConvert.SerializeObject(message));
    }
    
    internal void sendCloseProjectToWsServer()
    {
        var message = new { call = "closedProject" };
        SendMessageToServer(JsonConvert.SerializeObject(message));
    }
    
    public void SendMessageToServer(string message)
    {
        if (ws != null && isConnected)
        {
            ws.SendText(message);
        }
        else
        {
            Debug.Log("WebSocket is not connected. Message not sent.");
        }
    }
    
    private async void ConnectToServer()
    {
        string jwtToken = PlayerPrefs.GetString("web_auth_token", string.Empty);
        if (!string.IsNullOrEmpty(jwtToken))
        {
            ws = new WebSocket($"{ServerConfigurator.defaultWebsocketRoot}?clienttype=player&token={System.Uri.EscapeDataString(jwtToken)}&clientId={PlayerPrefs.GetString("client_id")}");
            
            ws.OnOpen += () =>
            {
                Debug.Log("Connected to websocket server.");
                isConnected = true;
            };

            ws.OnMessage += (bytes) =>
            {
                ServerEvents(bytes);
            };

            ws.OnClose += (e) =>
            {
                Debug.Log("Disconnected from server.");
                isConnected = false;
            };

            ws.OnError += (e) =>
            {
                Debug.LogError("Error in WebSocket connection: " + e);
            };

            await ws.Connect();
        }
        else
        {
            Debug.LogError("JWT-Token nicht gefunden. Stellen Sie sicher, dass Sie angemeldet sind.");
        }
    }

    internal async void CloseConnection()
    {
        if (ws != null)
        {
            await ws.Close();
        }
    }
    
    void Update()
    {
        if (ws != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            ws.DispatchMessageQueue();
#endif
        }
    }

    void OnDestroy()
    {
        CloseConnection();
    }
}