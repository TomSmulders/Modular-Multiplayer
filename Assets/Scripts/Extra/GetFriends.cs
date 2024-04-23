using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GetFriends : MonoBehaviour
{
    #region variables

    public static GetFriends instance;
    [SerializeField]  GameObject friendCardPrefab, friendCardParent;
    public List<GameObject> friendsCardsOnline = new List<GameObject>();
    public List<GameObject> friendsCardsOfline = new List<GameObject>();

    #endregion
    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }
    private void Start()
    {
        Request_Friends(true);
    }

    public async void Request_Friends(bool visible)
    {
        foreach (Transform card in friendCardParent.transform)
        {
            Destroy(card.gameObject);
        }

        friendsCardsOnline.Clear();
        friendsCardsOfline.Clear();

        if (visible)
        {
            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (friend.IsOnline)
                {
                    friendsCardsOnline.Add(await Create_FriendsCard(friend));
                }
                if (!friend.IsOnline)
                {
                    friendsCardsOfline.Add(await Create_FriendsCard(friend));
                }
            }

            foreach (var card in friendsCardsOnline)
            {
                card.transform.SetParent(friendCardParent.transform);
                Debug.Log("create");
            }
            foreach (var card in friendsCardsOfline)
            {
                card.transform.SetParent(friendCardParent.transform);
            }
        }
    }
    public async Task<GameObject> Create_FriendsCard(Friend friend)
    {
        GameObject card = Instantiate(friendCardPrefab);

        FriendInfo data = card.GetComponent<FriendInfo>();
        data.steamName = friend.Name;
        data.steamId = friend.Id;
        data.profileImage.texture = await GameManager.instance.Get_User_Profile_Picture(friend.Id);

        data.Update_FriendData();

        return(card);
    }
}
