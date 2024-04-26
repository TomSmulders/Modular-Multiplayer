using UnityEngine;

public static class WavUtility
{
    // Create an AudioClip from raw PCM audio data (16-bit, mono)
    public static AudioClip ToAudioClip(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogWarning("Audio data is null or empty.");
            return null;
        }

        // Create a new AudioClip
        AudioClip audioClip = AudioClip.Create("VoiceClip", audioData.Length / 2, 1, 16000, false);

        // Set the audio clip data
        audioClip.SetData(ConvertByteDataToFloatArray(audioData), 0);

        return audioClip;
    }

    // Convert raw PCM audio data (16-bit, little-endian) to float array (-1.0f to 1.0f)
    private static float[] ConvertByteDataToFloatArray(byte[] audioData)
    {
        float[] floatArray = new float[audioData.Length / 2];

        for (int i = 0; i < floatArray.Length; i++)
        {
            short sample = (short)((audioData[i * 2 + 1] << 8) | audioData[i * 2]);
            floatArray[i] = sample / 32768.0f; // Convert to float (-1.0f to 1.0f)
        }

        return floatArray;
    }
}
