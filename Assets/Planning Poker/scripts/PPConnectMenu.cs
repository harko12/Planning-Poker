using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TNet;
using LitJson;
using UnityEngine.SceneManagement;

public class PPConnectMenu : MonoBehaviour {

    public string mAddress = "71.206.244.109";
    public System.Collections.Generic.List<string> addressList = new System.Collections.Generic.List<string>();
    public string gameScene;
    public string connectScene;
    private string playerName;
    public UIButton btnConnect;
    public UILabel lblStatus;
    public UIInput inpPlayerName;
    public UIInput inpAddress;
    public UIPopupList popAddressList;

    private UILabel btnLabel;


    private bool nameSet = false;
    private const string playerNameKey = "PlanningPoker_PlayerName";
    private const string addressKey = "PlanningPoker_Address";
    private const string addressListKey = "PlanningPoker_AddressList";
    // Use this for initialization
	void Start () {
        btnLabel = btnConnect.gameObject.GetComponentInChildren<UILabel>();
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            playerName = PlayerPrefs.GetString(playerNameKey);
            inpPlayerName.value = playerName;
            TNManager.playerName = playerName;
            nameSet = true;
        }

        if (PlayerPrefs.HasKey(addressListKey))
        {
            var addressListString = PlayerPrefs.GetString(addressListKey);
            if (addressListString == "")
            {
                addressList = new System.Collections.Generic.List<string>();
            }
            else
            {
                addressList = JsonMapper.ToObject<System.Collections.Generic.List<string>>(addressListString);
                popAddressList.Clear();
                foreach (var a in addressList)
                {
                    popAddressList.AddItem(a);
                }
            }
        }

        if (PlayerPrefs.HasKey(addressKey))
        {
            mAddress = PlayerPrefs.GetString(addressKey);
        }

        inpAddress.value = mAddress;
        popAddressList.value = mAddress;

        if (Application.isPlaying)
        {
            // Start resolving IPs
            Tools.ResolveIPs(null);

        }
	
	}
	
	// Update is called once per frame
	void Update () {
        bool connectEnabled = true;
        string btnText = "Connect";
        if (string.IsNullOrEmpty(inpPlayerName.value))
        {
            connectEnabled = false;
            btnText = "Choose A Name";
        }
        btnConnect.enabled = connectEnabled;
        btnLabel.text = btnText;
	
	}

    public void OnConnectClicked()
    {
        var address = inpAddress.value;
        playerName = inpPlayerName.value;
        // set the player pref for the name
        PlayerPrefs.SetString(playerNameKey, playerName);
        // change the players name
        TNManager.playerName = playerName;

        PlayerPrefs.SetString(addressKey, address);
        if (!addressList.Contains(address))
        {
            addressList.Add(address);
            PlayerPrefs.SetString(addressListKey, JsonMapper.ToJson(addressList));
        }

        btnConnect.enabled = false;
        lblStatus.text = "Connecting..";
        TNManager.Connect(address);
    }

    public void OnAddressClicked()
    {
        mAddress = popAddressList.value;
        inpAddress.value = mAddress;
    }

    void OnNetworkConnect(bool success, string message)
    {
        if (success)
        {
            lblStatus.text = "Connected!";
            TNManager.JoinChannel(1, gameScene);
        }
        else
        {
            var msg = string.Format("Connection Failed: {0}", message);
            lblStatus.text = msg;
            btnConnect.enabled = true;
        }

    }

    void OnNetworkLeaveChannel()
    {
        SceneManager.LoadScene(connectScene);
    }
}
