using UnityEngine;
using System.Collections;
using TNet;

public class Dealer : Participant
{
    new public static Dealer instance;
    public int dealerId;
    public UIButton goButton;
    public UIButton pauseButton;
    public UIButton stopButton;
    public UIInput gameSecondsInput;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        myTransform = transform;
        var dealerPos = GameObject.Find("DealerPosition") as GameObject;
        PlanningPokerTools.MakeGameObjectChildOf(gameObject, dealerPos);
        if (tno.isMine)
        {
            values = new ParticipantValues("<none>", "", "Waiting..");
        }
        CanInput = false;
    }

    void OnNetworkPlayerJoin(Player p)
    {
        tno.Send("OnSetLabels", p, nameLabel.text, null, actionLabel.text);
        tno.Send("OnSetDealerId", p, dealerId);
    }

    new public void OnValueChosen()
    {
        Dealer.instance.values = new ParticipantValues(null, UIPopupList.current.value, null);

    }

    public void SetPlayerAsDealer(Player p)
    {
        tno.Send("OnSetLabels", Target.AllSaved, p.name, null, "Start Hand");
        tno.Send("OnSetDealerId", Target.AllSaved, p.id);
        gameSecondsInput.value = Timer.instance.GameSeconds.ToString();
        foreach (Participant part in PlanningPoker.instance.participants)// TNManager.FindObjectsOfType<Participant>())
        {
            bool isDealer = false;
            if (part.tno.ownerID == p.id)
                isDealer = true;

            part.isDealer = isDealer;
        }

    }

    [RFC]
    public void OnSetDealerId(int Id)
    {
        dealerId = Id;
        PlanningPoker.instance.elementPivot.MoveDealer(Id == TNManager.playerID);
    }

    [RFC]
    public void InitForPlayer(Player p)
    {
       // gameObject.name = "Dealer " + (p.id == TNManager.playerID ? "(me)" : "(them)");

    }

}
