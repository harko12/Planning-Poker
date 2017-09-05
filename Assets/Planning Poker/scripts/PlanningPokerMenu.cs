using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlanningPokerMenu : MonoBehaviour {

    public UILabel lblName;
    public UILabel letsGoLabel;
    public UILabel observingLabel;
	// Use this for initialization
	void Start () {
        lblName.text = TNManager.playerName;
        observingLabel.enabled = false;
	}
	
    /*
    public void UpdatePlayerName()
    {
        TNManager.playerName = nameInput.value;
    }
    */
    public void UpdateLetsButton(bool Observe)
    {
        var message = "Let's Play!";
        if (Observe)
            message = "Let's Watch!";

        letsGoLabel.text = message;
        observingLabel.enabled = Observe;
    }

    public void OnDisconnectClick()
    {
        TNManager.Disconnect();
    }

    /// <summary>
    /// this is the cheap way.. make it better
    /// </summary>
    void OnNetworkDisconnect()
    {
        SceneManager.LoadScene(0);
    }

}
