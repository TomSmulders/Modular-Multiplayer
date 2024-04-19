using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetFriends : MonoBehaviour
{
    public static GetFriends instance;
    [Header("carts")]
    [SerializeField] private GameObject friendCardPrefab, friendCardParent;
    public List<GameObject> friendsCards = new List<GameObject>();

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }
    



    public async void RequestFriends()
    {
        foreach (GameObject card in friendsCards)
        {
            Destroy(card);
        }

        foreach (Friend friend in SteamFriends.GetFriends())
        {
            if (friend.IsOnline)
            {
                CreateFriendCard(friend);
            }
            if (!friend.IsOnline)
            {
                // Do stuff
            }
        }
    }



    public async void CreateFriendCard(Friend friend)
    {
        GameObject card = Instantiate(friendCardPrefab);
        card.transform.SetParent(friendCardPrefab.transform);

        PlayerInfo data = card.GetComponent<PlayerInfo>();
        data.steamName = friend.Name;
        data.steamId = friend.Id;
        data.profileImage.texture = await GameManager.instance.GetProfilePicture(friend.Id);

        friendsCards.Add(card);
    }
}
