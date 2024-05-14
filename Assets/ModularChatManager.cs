using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Steamworks;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;
using System.Linq;
using System.Runtime.CompilerServices;

public class ModularChatManager : NetworkBehaviour
{
    public bool useSteamUsername = true;
    public string username = "user";
    public Color myUsernameColor = Color.white;
    public Color defaultUserChatColor = Color.white;
    public Color defaultPersonalChatColor = Color.gray;

    public List<ulong> accesIds = new List<ulong>();

    public bool multipleChats;

    public GameObject chatContentBoxes;
    public InputField chatInput;

    public string commandPrefix;


    public List<ChatCommand> globalChatCommands = new List<ChatCommand>();
    public List<ChatCommand> personalChatCommands = new List<ChatCommand>();
    public List<ChatCommand> teamChatCommands = new List<ChatCommand>();
    public List<ChatCommand> allChatsCommands = new List<ChatCommand>();



    private void Start()
    {
        //roep het voorbeeld command
        RunCommand("/kick kyan");
    }

    //voorbeeld command
    public void kick(ChatCommand command)
    {
        ChatCommandVariable player = command.GetVariableByName("player");
        Debug.Log(player.variableValue + " was kicked!");

        //logica om hun te kicken
    }

    public void teleport(ChatCommand command)
    {
        ChatCommandVariable username = command.GetVariableByName("username");
        ChatCommandVariable position = command.GetVariableByName("position");

        Debug.Log("I teleported " + username.variableValue + " to " + position.variableValue);
    }




    public void SetUserNickname(string _n){ username = _n;  }
    public void SetUserNicknameColor(Color _c){ myUsernameColor = _c;  }
    public void SetUserChatColor(Color _c){ defaultUserChatColor = _c;  }


    public void ReceiveChat() { }



    //global = name : message
    //personal = name1 -> me : message
    //team = {teamColor} name : message

    public void CreateGlobalChat(List<ulong> _users)
    {
        I_Want_To_Create_A_Chat_ServerRpc(SerializeListToJson(_users), defaultUserChatColor, ChatType.Global, "Global");
    }

    public void CreatePersonalChat(ulong _otherUser , string _otherUsername)
    {
        List<ulong> _users = new List<ulong>(){ _otherUser, NetworkManager.Singleton.LocalClientId };
        string _chatName = _otherUsername + ',' + username;
        I_Want_To_Create_A_Chat_ServerRpc(SerializeListToJson(_users), defaultPersonalChatColor, ChatType.Personal , _chatName);
    }

    public void CreateTeamChat(List<ulong> _users, Color _teamColor , string _teamName)
    {
        I_Want_To_Create_A_Chat_ServerRpc(SerializeListToJson(_users), _teamColor, ChatType.Team , _teamName);
    }


    //chat which will get all messages combined
    public void CreateCombinedChat() { }



    [ServerRpc]
    public void I_Want_To_Create_A_Chat_ServerRpc(string _usersInChatString, Color _chatColor , ChatType _chatType , string _chatName)
    {
        List<ulong> _usersInChat = DeserializeJsonToList(_usersInChatString);

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


        if (_usersInChat.Contains(NetworkManager.LocalClientId))
        {
            if(_chatType == ChatType.Personal)
            {
                string[] usernames = _chatName.Split(',');
                _chatName = NetworkManager.LocalClientId == _usersInChat[0] ? usernames[1] : usernames[0];
            }
            CreateChat_ClientRpc(_usersInChatString, _chatColor, _chatType, _lobbyChatAmmount, _chatName);
        }
    }

    [ClientRpc]
    void CreateChat_ClientRpc(string _usersInChat , Color _chatColor, ChatType _chatType, int _chatId, string _chatName)
    {
        ChatSettings chat = new ChatSettings(_chatName,_chatId, _chatColor, _chatType, DeserializeJsonToList(_usersInChat), commandPrefix);
    }



    private string SerializeListToJson(List<ulong> list)
    {
        List<string> stringList = new List<string>();
        foreach (ulong value in list)
        {
            stringList.Add(value.ToString());
        }

        string jsonString = JsonUtility.ToJson(stringList);
        return jsonString;
    }
    private List<ulong> DeserializeJsonToList(string jsonString)
    {
        List<string> stringList = JsonUtility.FromJson<List<string>>(jsonString);

        List<ulong> ulongList = new List<ulong>();
        foreach (string str in stringList)
        {
            ulong value;
            if (ulong.TryParse(str, out value))
            {
                ulongList.Add(value);
            }
            else
            {
                Debug.LogError("Failed to parse ulong value from string: " + str);
            }
        }

        return ulongList;
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


public class ChatSettings : MonoBehaviour
{
    public ChatSettings(string _n , int _id, Color _c, ChatType _t, List<ulong> _u, string _p)
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
    public Color chatColor;

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