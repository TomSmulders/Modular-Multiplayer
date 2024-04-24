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

    private string selectedMicrophone = "Mixer (Maonocaster C2 Neo)";

    void Start()
    {
        foreach (var item in Microphone.devices)
        {
            Debug.Log(item);
        }

        // Check if the selected microphone is available
        if (Array.Exists(Microphone.devices, device => device == selectedMicrophone))
        {
            // Start recording from the selected microphone
            audioClip = Microphone.Start(selectedMicrophone, true, 1000, 44100);
        }
        else
        {
            Debug.LogError("Selected microphone not found!");
        }
    }

    void Update()
    {
        // Check if the microphone is recording
        if (Microphone.IsRecording(selectedMicrophone))
        {
            // Convert AudioClip data to byte array
            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);

            int sampleCount = audioData.Length;
            int byteCount = sampleCount * sizeof(float);
            byte[] byteData = new byte[byteCount];

            Buffer.BlockCopy(audioData, 0, byteData, 0, byteCount);

            // Send audio data to clients
            SendMessageToClients(byteData);
        }
    }

    private void SendMessageToClients(byte[] byteData)
    {
        // Check if connected to a server as a client and not muted
        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient && !isMuted)
        {
            SendMessageToClientServerRpc(NetworkManager.LocalClientId, byteData);
        }
        else
        {
            Debug.LogWarning("Not connected to a server or client is muted.");
        }
    }

    [ServerRpc]
    private void SendMessageToClientServerRpc(ulong localClientId, byte[] byteData)
    {
        if (byteData == null || byteData.Length == 0)
        {
            Debug.LogWarning("Byte data is null or empty. Aborting send.");
            return;
        }

        // Set a reasonable maximum packet size (e.g., 1024 bytes)
        int maxPacketSize = 1024;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            // Skip sending audio data to the server itself
            if (clientId != localClientId)
            {
                int offset = 0;

                while (offset < byteData.Length)
                {
                    int count = Math.Min(maxPacketSize, byteData.Length - offset);
                    byte[] packetData = new byte[count];
                    Buffer.BlockCopy(byteData, offset, packetData, 0, count);
                    offset += count;

                    TargetReceiveAudioDataClientRpc(clientId, packetData);
                }
            }
        }
    }

    [ClientRpc]
    private void TargetReceiveAudioDataClientRpc(ulong clientId, byte[] byteData)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Only process audio data intended for the local client
            Debug.Log("Receiving audio data from server.");

            // Check if the byte data is valid
            if (byteData != null && byteData.Length > 0)
            {
                ClientAudioReceiver.Instance.ReceiveAudioData(byteData);
            }
            else
            {
                Debug.LogWarning("Received invalid audio data.");
            }
        }
    }

    void OnDestroy()
    {
        // Stop the microphone recording
        Microphone.End(selectedMicrophone);
    }
}
