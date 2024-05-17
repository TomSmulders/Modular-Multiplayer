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

public class ModularChatManager : NetworkBehaviour
{
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

    private void Awake()
    {
        if (GlobalGameSettings.instance.currentLobby.HasValue)
        {
            currentLobby = GlobalGameSettings.instance.currentLobby.Value;
        }
    }

    private void Start()
    {
        //roep het voorbeeld command
        //RunCommand("/kick kyan");
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


    public void SendMessage()
    {

    }


    public void ReceiveChat() { }



    //global = name : message
    //personal = name1 -> me : message
    //team = {teamColor} name : message


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && IsHost)
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
    }
    
    public void CreateGlobalChat(List<ulong> _users)
    {
        Debug.Log("length : " + _users.Count);
        Debug.Log("serialized : " + SerializeList(_users));
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


    private string SerializeList(List<ulong> ulongList)  
    {
        string stringlist = "";
        foreach (var item in ulongList)
        {
            stringlist += item.ToString() + ",";
        }
        return stringlist;
    }

    private List<ulong> DeserializeList(string stringlist)
    {
        List<ulong> ulongList = new List<ulong>();
        string[] stringArray = stringlist.Split(",");

        foreach (var id in stringArray)
        {
            ulongList.Add(ulong.Parse(id));
        }

        return ulongList;
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
        Debug.Log("Received usersInChatJson: " + _usersInChatJson); // Debugging

        List<ulong> _usersInChat = DeserializeList(_usersInChatJson); 

        Debug.Log("Length of usersInChat: " + _usersInChat.Count); // Debugging

        foreach (var item in _usersInChat)
        {
            Debug.Log(item); // Debugging
        }

        if (_usersInChat.Contains(NetworkManager.LocalClientId))
        {
            if (_chatType == ChatType.Personal)
            {
                string[] usernames = _chatName.Split(',');
                _chatName = NetworkManager.LocalClientId == _usersInChat[0] ? usernames[1] : usernames[0];
            }
            ChatSettings chat = new ChatSettings(_chatName, _chatId, _chatColor, _chatType, _usersInChat, commandPrefix);
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
    public ChatSettings(string _n , int _id, UnityEngine.Color _c, ChatType _t, List<ulong> _u, string _p)
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

    public ChatType chatType;

    public List<ulong> chatUsers = new List<ulong>();
    public List<string> chatHistory = new List<string>();

    public string chatCommandPrefix; // <-- character before the command (like --> /tp)
    public List<ChatCommand> chatCommands = new List<ChatCommand>();//commands
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