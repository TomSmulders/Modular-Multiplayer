using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using System;
using UnityEditor.VersionControl;
using UnityEngine.Analytics;

public class ModularChatManager : NetworkBehaviour
{
    public static ModularChatManager instance;

    public bool useSteamUsername = true;
    public string username = "user";
    public UnityEngine.Color myUsernameColor = UnityEngine.Color.white;
    public UnityEngine.Color defaultGlobalChatColor = UnityEngine.Color.white;
    public UnityEngine.Color defaultPersonalChatColor = UnityEngine.Color.gray;

    public List<ulong> accesIds = new List<ulong>();

    public bool multipleChats;

    public GameObject chatContentBoxes;
    public InputField chatInput;

    public string commandPrefix;

    public Lobby? currentLobby;

    public List<ChatSettings> chats = new List<ChatSettings>();

    public List<ChatCommand> globalChatCommands = new List<ChatCommand>();
    public List<ChatCommand> personalChatCommands = new List<ChatCommand>();
    public List<ChatCommand> teamChatCommands = new List<ChatCommand>();
    public List<ChatCommand> allChatsCommands = new List<ChatCommand>();

    public GameObject messagePrefab;
    public GameObject chatboxPrefab;
    public GameObject chatboxParent;
      
    public string tempMessage;
    public int tempChatID;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this); }
        if (GlobalGameManager.instance.currentLobby.HasValue)
        {
            currentLobby = GlobalGameManager.instance.currentLobby.Value;
        }
    }

    private void Start()
    {
        //roep het voorbeeld command
        //RunCommand("/kick kyan");

        RunCommand("/tp kyan tom");
    }

    public void TP_command(ChatCommand command)
    {
        ChatCommandVariable player1 = command.GetVariableByName("player1");
        ChatCommandVariable player2 = command.GetVariableByName("player2");

        Debug.Log(player1.variableValue + " Tped to " + player2.variableValue);

        //do whatever you want

    }


    //voorbeeld command
    public void kick(ChatCommand command)
    {
        ChatCommandVariable player = command.GetVariableByName("player");
        Debug.Log(player.variableValue + " was kicked!");

        //logica om hun te kicken
    }


    public void SetUserNickname(string _n){ username = _n;  }
    public void SetUserNicknameColor(UnityEngine.Color _c){ myUsernameColor = _c;  }
    public void SetUserChatColor(UnityEngine.Color _c){ defaultGlobalChatColor = _c;  }

    public void Connect_Chat(Lobby _lobby)
    {
        currentLobby = _lobby;
    }


    public void ReceiveChat() { }



    //global = name : message
    //personal = name1 -> me : message
    //team = {teamColor} name : message


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Trying to create chat");
            if (currentLobby.HasValue)
            {
                CreateGlobalChat(NetworkManager.ConnectedClientsIds.ToList());
            }
            else
            {
                Debug.Log("You are not connected to a lobby, Please call ModularChatManager.Connect_Chat(Lobby);");
            }

        }
        else if(Input.GetKeyDown(KeyCode.S))
        {
            if (currentLobby.HasValue)
            {
                SendMessageToChat(tempMessage,username,tempChatID);
            }
        }
    }
    
    public void SendMessageToChat(string _message, string _sender, int _chatId)
    {
        ChatSettings chat = GetChatById(_chatId);
        if(chat != null)
        {
            I_WantTo_Send_A_Message_ServerRpc(SerializeList<ulong>(chat.chatUsers), _message, _sender, _chatId);
        }
    }

    public void CreateGlobalChat(List<ulong> _users)
    {
        I_Want_To_Create_A_Chat_ServerRpc(SerializeList(_users), defaultGlobalChatColor, ChatType.Global, "Global");
    }

    public void CreatePersonalChat(ulong _otherUser , string _otherUsername)
    {
        List<ulong> _users = new List<ulong>(){ _otherUser, NetworkManager.Singleton.LocalClientId };
        string _chatName = _otherUsername + ',' + username;
        I_Want_To_Create_A_Chat_ServerRpc(SerializeList(_users), defaultPersonalChatColor, ChatType.Personal , _chatName);
    }

    public void CreateTeamChat(List<ulong> _users, UnityEngine.Color _teamColor , string _teamName)
    {
        I_Want_To_Create_A_Chat_ServerRpc(SerializeList(_users), _teamColor, ChatType.Team , _teamName);
    }


    //chat which will get all messages combined
    public void CreateCombinedChat() { }


    private string SerializeList<T>(List<T> standartList)  
    {
        string stringlist = "";
        for (int i = 0; i < standartList.Count; i++)
        {
            if(i < standartList.Count - 1)
            {
                stringlist += standartList[i].ToString() + ",";
            }
            else
            {
                stringlist += standartList[i].ToString();
            }
        }

        return stringlist;
    }

    private List<T> DeserializeList<T>(string stringlist)
    {
        List<T> standartList = new List<T>();
        string[] stringArray = stringlist.Split(",");

        foreach (var id in stringArray)
        {
            if (typeof(T) == typeof(ulong))
            {
                ulong parsedValue;
                if (ulong.TryParse(id, out parsedValue))
                    standartList.Add((T)Convert.ChangeType(parsedValue, typeof(T)));
            }
            else if (typeof(T) == typeof(int))
            {
                int parsedValue;
                if (int.TryParse(id, out parsedValue))
                    standartList.Add((T)Convert.ChangeType(parsedValue, typeof(T)));
            }
            else if (typeof(T) == typeof(float))
            {
                float parsedValue;
                if (float.TryParse(id, out parsedValue))
                    standartList.Add((T)Convert.ChangeType(parsedValue, typeof(T)));
            }
            else if (typeof(T) == typeof(double))
            {
                double parsedValue;
                if (double.TryParse(id, out parsedValue))
                    standartList.Add((T)Convert.ChangeType(parsedValue, typeof(T)));
            }
        }

        return standartList;
    }

    [ServerRpc(RequireOwnership = false)]
    public void I_WantTo_Send_A_Message_ServerRpc(string _usersToSendToSerialized, string _message, string _sender, int _chatId)
    {
        Send_Message_ClientRpc(_usersToSendToSerialized, _message, _sender, _chatId);
    }

    [ClientRpc]
    public void Send_Message_ClientRpc(string _usersToSendToSerialized, string _message, string _sender, int _chatId)
    {
        List<ulong> _usersToSendTo = DeserializeList<ulong>(_usersToSendToSerialized);

        if (_usersToSendTo.Contains(NetworkManager.LocalClientId))
        {
            ChatSettings chat = GetChatById(_chatId);
            if (chat != null)
            {
                chat.SendMessage(_message, _sender,chat.chatColor);
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void I_Want_To_Create_A_Chat_ServerRpc(string _usersInChatJson, UnityEngine.Color _chatColor , ChatType _chatType , string _chatName)
    { 
        int _lobbyChatAmmount = 0;
        if (int.TryParse(GameNetworkManager.instance.currentLobby.Value.GetData("lobbyChatAmmount"), out _lobbyChatAmmount))
        {
            _lobbyChatAmmount++;
        }
        else
        {
            _lobbyChatAmmount = 1;
        }
        GameNetworkManager.instance.currentLobby.Value.SetData("lobbyChatAmmount", _lobbyChatAmmount.ToString());

        CreateChat_ClientRpc(_usersInChatJson, _chatColor, _chatType, _lobbyChatAmmount, _chatName);
    }

    [ClientRpc]
    void CreateChat_ClientRpc(string _usersInChatJson, UnityEngine.Color _chatColor, ChatType _chatType, int _chatId, string _chatName)
    {
        List<ulong> _usersInChat = DeserializeList<ulong>(_usersInChatJson); 

        if (_usersInChat.Contains(NetworkManager.LocalClientId))
        {
            if (_chatType == ChatType.Personal)
            {
                string[] usernames = _chatName.Split(',');
                _chatName = NetworkManager.LocalClientId == _usersInChat[0] ? usernames[1] : usernames[0];
            }
            ChatSettings chat = new ChatSettings(_chatName, _chatId, _chatColor, _chatType, _usersInChat, commandPrefix);

            chat.chatGameObject = Instantiate(chatboxPrefab);
            chat.chatGameObject.transform.SetParent(chatboxParent.transform);
            chat.chatGameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            chat.chatGameObject.name = chat.chatName;

            chat.chatCommands.AddRange(allChatsCommands);
            switch (chat.chatType)
            {
                case ChatType.Global: chat.chatCommands.AddRange(globalChatCommands); break;
                case ChatType.Personal: chat.chatCommands.AddRange(personalChatCommands); break;
                case ChatType.Team: chat.chatCommands.AddRange(teamChatCommands); break;
                case ChatType.Combined:
                    chat.chatCommands.AddRange(globalChatCommands);
                    chat.chatCommands.AddRange(personalChatCommands);
                    chat.chatCommands.AddRange(teamChatCommands); break;
            }

            chats.Add(chat);

            Debug.Log("I created a chat");
        }
    }

    public void RunCommand(string commandText)
    {
        //Eerst checkt hij of de command prefix ervoor staat
        if (commandText[0].ToString() == commandPrefix)
        {
            commandText = commandText.Substring(1);//haalt de prefix weg

            string commandName = commandText.Split(" ")[0];//split het eerste woord en pakt de command name
            commandText = commandText.Substring(commandName.Length+1);//removed command name van command text

            //Vind het juiste command voor de variable volgorde
            ChatCommand command = GetCommandByName(commandName);

            //Checkt of jij (client) het command wel mag uitvoeren
            if (accesIds.Contains(command.accessId) && command.requiresAccess || !command.requiresAccess)
            {
                foreach (var variable in command.variables)//checkt wat voor variable het is en dan zet hij de value + verwijdert het van de commandText totdat hij langs al de variables is geweest
                {
                    if (variable.variableType == ChatVariableType.STRING || variable.variableType == ChatVariableType.NUMBER || variable.variableType == ChatVariableType.HEX)
                    {
                        variable.variableValue = commandText.Split(" ")[0];
                        commandText = commandText.Substring(variable.variableValue.Length);
                    }
                    else if (variable.variableType == ChatVariableType.VECTOR)
                    {
                        variable.variableValue = ExtractFirstVector(commandText);
                        commandText = commandText.Substring(variable.variableValue.Length + 2);
                    }
                }

                command.RunCommand();//Run een custom unity event dat ik heb gemaakt
            }
        }
    }

    ChatCommand GetCommandByName(string name)
    {
        foreach (var command in globalChatCommands) // VERANDEREN NAAR chatsettins.chatCommands LATER!!!!!!!
        {
            if(command.name.ToLower() == name.ToLower())
            {
                return command;
            }
        }
        Debug.Log("this command does not exist");
        return null;
    }

    ChatSettings GetChatById(int _id)
    {
        foreach (var chat in chats)
        {
            if(chat.chatId == _id)
            {
                return chat;
            }
        }
        return null;
    }

    string ExtractFirstVector(string input)
    {
        int startIndex = input.IndexOf('(');
        int endIndex = input.IndexOf(')');

        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex + 1)
        {
            int contentStartIndex = startIndex + 1;
            int contentLength = endIndex - contentStartIndex;
            string contentInsideParentheses = input.Substring(contentStartIndex, contentLength).Trim();
            return contentInsideParentheses;
        }

        return null;
    }
}

public enum ChatType { Global, Personal, Team, Combined };

[System.Serializable]
public class ChatSettings
{
    public ChatSettings(string _n, int _id, UnityEngine.Color _c, ChatType _t, List<ulong> _u, string _p)
    {
        this.chatName = _n;
        this.chatId = _id;
        this.chatColor = _c;
        this.chatType = _t;
        this.chatUsers = _u;
        this.chatCommandPrefix = _p;
    }

    public string chatName;
    public int chatId;
    public UnityEngine.Color chatColor;

    public GameObject chatGameObject;
    public GameObject messagesParent;

    public ChatType chatType;

    public List<ulong> chatUsers = new List<ulong>();
    public List<string> MessageHistory = new List<string>();
    public List<MessageScript> MessageData = new List<MessageScript>();

    public Dictionary<string, GameObject> chatHistory = new Dictionary<string, GameObject>();

    public string chatCommandPrefix; // <-- character before the command (like --> /tp)
    public List<ChatCommand> chatCommands = new List<ChatCommand>();//commands

    public void SendMessage(string _message,string _sender, UnityEngine.Color? _color)
    {
        if(messagesParent == null) { messagesParent = chatGameObject.GetComponentInChildren<VerticalLayoutGroup>().gameObject; }

        if(ModularChatManager.instance != null && ModularChatManager.instance.messagePrefab != null)
        {
            GameObject newMessage = GameObject.Instantiate(ModularChatManager.instance.messagePrefab);
            newMessage.transform.SetParent(this.messagesParent.transform);

            MessageScript messageScript = newMessage.GetComponent<MessageScript>();

            if (_color.HasValue)
            {
                messageScript.Init(_sender, _message, _color.Value);
            }
            else
            {
                messageScript.Init(_sender, _message, chatColor);
            }

            MessageHistory.Add(_sender + " : " + _message);
            MessageData.Add(messageScript);
        }
    }
}


[System.Serializable]
public class ChatCommand
{
    public string name;
    public bool requiresAccess;
    public ulong accessId;

    [Header("Command variables")]
    public List<ChatCommandVariable> variables = new List<ChatCommandVariable>();

    public ChatCommandVariable GetVariableByName(string varName)
    {
        foreach (var variable in variables)
        {
            if (variable.variableName.ToLower() == varName.ToLower()) { return variable; }
        }
        return null;
    }

    [Header("Command function")]
    public CustomCommandUnityEvent commandEvent = new CustomCommandUnityEvent();

    public void RunCommand()
    {
        commandEvent.Invoke(this);
    }
}

[System.Serializable]
public class CustomCommandUnityEvent : UnityEvent<ChatCommand>
{
}

// {prefix}{commandname} {variable} {variable} {variable} 

public enum ChatVariableType { STRING, NUMBER, HEX, VECTOR };
// string = thisisastring
// string[] = this is a string
// number = 0 || 0,2112398 
// hex = #037492 || 287489
// vector (number number) || (number number number)

[System.Serializable]
public class ChatCommandVariable
{
    public string variableName = "name";
    public ChatVariableType variableType;
    [HideInInspector]
    public string variableValue = "value";
}