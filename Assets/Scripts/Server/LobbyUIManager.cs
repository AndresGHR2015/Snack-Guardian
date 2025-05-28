using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private TextMeshProUGUI chatDisplay; // Change to TextMeshProUGUI
    [SerializeField] private TextMeshProUGUI playerList;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button sendMessageButton;
    
    private bool isReady = false;
    private List<string> connectedPlayers = new List<string>();

    private void Start()
    {
        startGameButton.interactable = false;
        readyButton.onClick.AddListener(onReadyClick);
        sendMessageButton.onClick.AddListener(sendChatMessage);
        
        // Subscribe to all events
        MultiplayerGameEvents.onConnectedToServer += handleConnectedToServer;
        MultiplayerGameEvents.onChatMessageReceived += handleChatMessage;
        MultiplayerGameEvents.onPlayerConnected += handlePlayerConnected;
        MultiplayerGameEvents.onPlayerDisconnected += handlePlayerDisconnected;

        // Initialize chat display
        chatDisplay.text = "Chat initialized...";
    }

    public void onReadyClick()
    {
        isReady = !isReady;
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Unready" : "Ready";
        Debug.Log($"Player is now {(isReady ? "ready" : "not ready")}");
        
        // Notify server about readiness
        NetworkManager.Instance.sendReadyState(isReady);
        
        // Update UI based on readiness
        startGameButton.interactable = isReady && connectedPlayers.Count > 1;
    }

    private void handlePlayerDisconnected(string obj)
    {
        throw new NotImplementedException();
    }

    public void sendChatMessage()
    {
        if (!string.IsNullOrEmpty(chatInput.text))
        {
            Debug.Log($"Sending message: {chatInput.text}"); // Debug line
            NetworkManager.Instance.sendPublicMessage(chatInput.text);
            chatInput.text = "";
        }
    }

    private void handleChatMessage(string playerId, string message)
    {
         Debug.Log($"Handling chat message: {playerId}: {message}"); // Debug line
        
        // Ensure UI updates happen on main thread
        if (!this) return;

        chatDisplay.text += $"\n{playerId}: {message}";
        Canvas.ForceUpdateCanvases(); // Force UI refresh
    }

    private void handleConnectedToServer()
    {
        Debug.Log("Connected to server"); // Debug line
        chatDisplay.text = "Connected to server. Waiting for players...";
        connectedPlayers.Clear();
        updatePlayerList();     
        // Notify server about player connection
        NetworkManager.Instance.sendPublicMessage("Player has joined the lobby.");
    }

    private void handlePlayerConnected(string playerId)
    {
        Debug.Log($"Player connected: {playerId}"); // Debug line
        if (!connectedPlayers.Contains(playerId))
        {
            connectedPlayers.Add(playerId);
            updatePlayerList();
            chatDisplay.text += $"\n<color=green>Player {playerId} joined</color>";
        }
    }

    private void updatePlayerList()
    {
        playerList.text = "Connected Players:\n";
        foreach (var player in connectedPlayers)
        {
            playerList.text += $"{player}\n";
        }
    }

    private void OnDestroy()
    {
        MultiplayerGameEvents.onConnectedToServer -= handleConnectedToServer;
        MultiplayerGameEvents.onChatMessageReceived -= handleChatMessage;
        MultiplayerGameEvents.onPlayerConnected -= handlePlayerConnected;
        MultiplayerGameEvents.onPlayerDisconnected -= handlePlayerDisconnected;
    }
}