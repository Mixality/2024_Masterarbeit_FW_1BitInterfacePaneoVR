using System;
using UnityEngine;

public class UniqueIdentifierManager : MonoBehaviour
{
    private void Start()
    {
        InitializeClientID();
    }

    private void InitializeClientID()
    {
        // check if there is already a client id
        if (!PlayerPrefs.HasKey("client_id"))
        {
            // Generieren einer neuen UUID
            string clientId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("client_id", clientId);
            PlayerPrefs.Save();
        }
    }

    // Diese Methode könnte verwendet werden, um die client_id bei Anfragen mitzusenden
    public string GetClientID()
    {
        return PlayerPrefs.GetString("client_id");
    }
}