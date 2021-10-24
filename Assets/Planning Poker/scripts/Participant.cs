using UnityEngine;
using System.Collections;
using TNet;

public class ParticipantValues
{
    public string name;
    public string action;
    public string value;

    public ParticipantValues()
    {

    }

    public ParticipantValues(string n, string a, string v)
    {
        name = n;
        action = a;
        value = v;
    }
}
public class Participant : TNBehaviour {

    protected Transform myTransform;
    public ParticipantValues values
    {
        set
        {
            tno.Send("OnSetLabels", Target.AllSaved, value.name, value.action, value.value);
        }
    }
    public UILabel nameLabel;
    public UILabel actionLabel;
    public UILabel valueLabel;
    public UIPopupList valuePopup;
    public UISprite disableOverlay;
    public UITweener statusTween;
    public UISprite statusSprite;
    //private Color spriteStart;

    public static Participant instance;
    private bool mIsDealer = false;
    public bool isDealer
    {
        set
        {
            tno.Send("OnSetDealer", Target.AllSaved, value);
        }
        get
        {
            return mIsDealer;
        }
    }

    public void OnValueChosen()
    {
        if (Participant.instance != null && Participant.instance.CanInput)
            Participant.instance.values = new ParticipantValues(null, UIPopupList.current.value, null);
    }
    void Awake()
    {
        if (tno.isMine)
            instance = this;
    }
    // Use this for initialization
    void Start()
    {
        myTransform = transform;
        InitForHand();
        // register ourselves with the pokermanager
        PlanningPoker.instance.AddParticipant(this);
    }

    void OnDestroy()
    {
        var pp = PlanningPoker.instance;
        PlanningPokerTools.MakeGameObjectChildOf(gameObject, pp.holdingPanel.gameObject);
        PlanningPoker.instance.RemoveParticipant(this);
    }
    private void InitForHand()
    {
        if (tno.isMine)
        {
            if (Participant.instance != null)
            {
                Participant.instance.values = new ParticipantValues(TNManager.playerName, "", "Waiting..");
                UIPopupList list = Participant.instance.GetComponentInChildren<UIPopupList>();
                list.items = PlanningPoker.instance.pokerChoices;
            }
            
        }
       // statusSprite.enabled = false;
        CanInput = false;
    }

    [RFC]
    public void OnSetDealer(bool b)
    {
        if (this is Dealer)
        {
            if (Dealer.instance.dealerId == TNManager.playerID)
            {
                CanInput = true;
            }
        }
        else
        {
            mIsDealer = b;
        }
    }

    private bool mCanInput = false;
    public bool CanInput
    {
        get
        {
            return mCanInput;
        }
        set
        {
            mCanInput = value;
            SetCanInput(value);
        }
    }

    protected void SetCanInput(bool canInput)
    {
        disableOverlay.enabled = !canInput;
    }

    public IEnumerator StatusFlash()
    {
        string[] waitingAnim = new string[] { "...", "|..", ".|.", "..|", "...", "..|", ".|.", "|.." };
        int animIndex = 0;
        //statusSprite.enabled = true;
        statusTween.enabled = true;
        statusTween.PlayForward();
        var flash = true;
        while (flash)
        {
            var pct = Timer.instance.PercentComplete;
            if (valueChosen || RevealHand || pct >= 1)
                flash = false;
            
            if (tno.ownerID != TNManager.playerID)
            {
                var randomValue = waitingAnim[animIndex];
                valueLabel.text = randomValue.ToString();
                animIndex++;
                if (animIndex >= waitingAnim.Length)
                    animIndex = 0;
            }
            
            yield return new WaitForSeconds(.5f);
        }
        if (valueChosen)
        {
            valueLabel.text = ":)";
            statusTween.PlayReverse();
        }
        else
        {
            valueLabel.text = ":(";
        }
        yield return null;
    }

    [RFC]
    public void OnHandStarted()
    {
        RevealHand = false;
        mActualValue = null;
        var msg = "Choosing..";
        if (tno.isMine)
        {
            msg = "Choose a value.";
            CanInput = true;
        }
        OnSetLabels(null, "", msg);
        StartCoroutine("StatusFlash");
    }

    [RFC]
    public void OnHandStopped()
    {
        RevealHand = true;
        var msg = "";
        if (valueChosen)
        {
            msg = "Good Job!";
        }
        else
        {
            msg = "Too Late!";
        }
        if (tno.isMine)
        {
            CanInput = false;
        }
        OnSetLabels(null, ActualValue ?? "", msg);
    }

    private string mActualValue = null;
    public string ActualValue
    {
        get { return mActualValue; }
    }

    public bool valueChosen
    {
        get
        {
            //Debug.Log(string.Format("hasvalue for {0}: {1} from value {2}", gameObject.name, !string.IsNullOrEmpty(mActualValue), mActualValue));
            return !string.IsNullOrEmpty(mActualValue);
        }
    }
    private bool RevealHand = false;
	[RFC]
    public void OnSetLabels(string name, string value, string action)
    {
        if (name != null) nameLabel.text = name;
        if (value != null)
        {
            if (!string.IsNullOrEmpty(value))
            {
                mActualValue = value; // only set the value if it really is a value
            }
            if (tno.isMine || RevealHand)
            {
                valueLabel.text = value;
            }
            else
                valueLabel.text = "???";

        }
        if (action != null) actionLabel.text = action;
    }

    void OnNetworkPlayerJoin(Player p)
    {
        tno.Send("OnSetLabels", p, nameLabel.text, valueLabel.text, actionLabel.text);
    }

    void OnNetworkPlayerLeave(Player p)
    {

    }
}

static class PlanningPokerTools
{
    public static void MakeGameObjectChildOf(GameObject go, GameObject parent)
    {
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.parent = parent.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        else if (parent == null)
        {
            go.transform.parent = null;
        }
    }

}