using System;
using UnityEngine;

public class ClientAudioReceiver : MonoBehaviour
{
    public static ClientAudioReceiver Instance;

    void Awake()
    {
        if(Instance == null) { Instance = this; } else { Destroy(this); }
    }

    public void ReceiveAudioData(byte[] byteData)
    {
        // Convert received byte array to AudioClip
        AudioClip receivedAudioClip = ConvertBytesToAudioClip(byteData);

        // Play the received audio
        PlayReceivedAudio(receivedAudioClip);
    }

    private AudioClip ConvertBytesToAudioClip(byte[] byteData)
    {
        // Convert byte array to float array
        float[] audioData = new float[byteData.Length / 4];
        Buffer.BlockCopy(byteData, 0, audioData, 0, byteData.Length);

        // Create AudioClip from float array
        AudioClip audioClip = AudioClip.Create("ReceivedAudio", audioData.Length, 1, 44100, false);
        audioClip.SetData(audioData, 0);

        return audioClip;
    }

    private void PlayReceivedAudio(AudioClip audioClip)
    {
        // Play the received audio
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
