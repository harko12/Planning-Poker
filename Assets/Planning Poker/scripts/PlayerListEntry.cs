using UnityEngine;
using System.Collections;
using TNet;

public class PlayerListEntry : MonoBehaviour {

    private PlanningPoker pokerManager;
    public Player myPlayer;
    public UILabel nameText;
    public UILabel buttonText;
    public UIButton button;
	// Use this for initialization
    void Start()
    {
        pokerManager = GameObject.Find("_PlanningPokerManager").GetComponent<PlanningPoker>();
    }
    public void OnClick()
    {
        pokerManager.SetDealer(myPlayer);
    }

    public void UpdateLine(Player p)
    {
        if (p.id == Dealer.instance.dealerId)
        {
            buttonText.text = "Dealer";
            button.enabled = false;

        }
        else
        {
            buttonText.text = "Set as Dealer";
            button.enabled = true;
        }
        nameText.text = p.name;
    }

    public void DisableButton()
    {
        button.enabled = false;
        /*
        var col = GetComponentInChildren<Collider>();
        col.enabled = false;
         */
    }
}
