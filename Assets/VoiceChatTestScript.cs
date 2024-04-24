using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netcode;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using System;

public class VoiceChatTestScript : NetworkBehaviour
{
    private AudioClip audioClip;
    public bool isMuted = false;

    void Start()
    {
        // Start recording from the microphone
        audioClip = Microphone.Start(null, true, 0, 44100);
    }

    void Update()
    {
        // Check if the microphone is recording
        if (Microphone.IsRecording(null))
        {
            // Convert AudioClip data to byte array
            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);

            byte[] byteData = new byte[audioData.Length * 4];
            Buffer.BlockCopy(audioData, 0, byteData, 0, byteData.Length);

            // Send audio data to clients
            SendMessageToClients(byteData);
        }
    }

    private void SendMessageToClients(byte[] byteData)
    {
        // Check if connected to a server as a client and not muted
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient && !isMuted)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                // Skip sending audio data to the server itself
                if (clientId != NetworkManager.Singleton.LocalClientId)
                {
                    Debug.Log("Sending audio to client: " + clientId);
                    SendMessageToClientServerRpc(clientId, byteData);
                }
            }
        }
        else
        {
            Debug.LogWarning("Not connected to a server or client is muted.");
        }
    }

    [ServerRpc]
    private void SendMessageToClientServerRpc(ulong clientId, byte[] byteData)
    {
        TargetReceiveAudioDataClientRpc(clientId, byteData);
    }

    [ClientRpc]
    private void TargetReceiveAudioDataClientRpc(ulong clientId, byte[] byteData)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Only process audio data intended for the local client
            Debug.Log("Receiving audio data from server.");
            ClientAudioReceiver.Instance.ReceiveAudioData(byteData);
        }
    }

    void OnDestroy()
    {
        // Stop the microphone recording
        Microphone.End(null);
    }
}
