using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GetFriends : MonoBehaviour
{
    public static GetFriends instance;
    [Header("carts")]
    [SerializeField]  GameObject friendCardPrefab, friendCardParent;
    public List<GameObject> friendsCardsOnline = new List<GameObject>();
    public List<GameObject> friendsCardsOfline = new List<GameObject>();

    private void Awake()
    {
        if (instance != null) { Destroy(this); } else { instance = this; }
    }


    private void Start()
    {
        RequestFriends(true);
    }

    public async void RequestFriends(bool visible)
    {
        foreach (Transform card in friendCardParent.transform)
        {
            Destroy(card.gameObject);
        }

        friendsCardsOnline.Clear();
        friendsCardsOfline.Clear();

        if (visible)
        {
            Debug.Log("detroy card");

            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (friend.IsOnline)
                {
                    friendsCardsOnline.Add(await CreateFriendCard(friend));
                }
                if (!friend.IsOnline)
                {
                    friendsCardsOfline.Add(await CreateFriendCard(friend));
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



    public async Task<GameObject> CreateFriendCard(Friend friend)
    {
        GameObject card = Instantiate(friendCardPrefab);

        FriendInfo data = card.GetComponent<FriendInfo>();
        data.steamName = friend.Name;
        data.steamId = friend.Id;
        data.profileImage.texture = await GameManager.instance.GetProfilePicture(friend.Id);

        data.UpdateFriendData();

        return(card);
    }
}
