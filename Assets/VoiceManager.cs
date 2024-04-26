using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;
using System.Linq;


public class VoiceManager : MonoBehaviour
{
    public static VoiceManager instance;

    public string currentChannel;

    private void Awake()
    {
        if(instance == null) { instance = this; } else { Destroy(this); }
        InitializeAsync();
    }
    async void InitializeAsync()
    {
        if (!VivoxService.Instance.IsLoggedIn)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            await VivoxService.Instance.InitializeAsync();
        }
    }

    public async void Login()
    {
        LoginOptions options = new LoginOptions();
        options.DisplayName = SteamClient.Name;
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
    }

    private void OnApplicationQuit()
    {
        LeaveChannel();
    }

    public async void Logout()
    {
        await VivoxService.Instance.LogoutAsync();
    }

    public async void JoinChannel(string channelToJoin)
    {
        Login();
        await VivoxService.Instance.JoinEchoChannelAsync(channelToJoin, ChatCapability.TextAndAudio);
        currentChannel = channelToJoin;
    }
    public async void LeaveChannel()
    {
        if (currentChannel != string.Empty)
        {
            await VivoxService.Instance.LeaveChannelAsync(currentChannel);
        }
        Logout();
    }
}
