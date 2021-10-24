using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SideBetPanel : MonoBehaviour {
    public static SideBetPanel instance;

    public UITweener sideBetCameraTween;
    public UITweener sideBetPosTween;
    public LookAtTarget cameraLook;
    public Transform sideBetPos;

    private Transform mCurrentCameraTarget;

    public UIPanel payoutPanel;
    public UILabel lblName, lblMoney, lblCurrentHistory, lblBetHistory, lblBetResult, lblPayoutText;
    public UIInput BetAmount;
    public UIToggle PlayerChosen, ResultChosen;
    public UIButton btnMakeBet, btnOpenPanel, btnPlayerList, btnPlayerChoice, btnFinalResult;
    public UIPopupList PlayerList, PlayerChoice, FinalResult;

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        btnOpenPanel.GetComponent<TweenPosition>().PlayReverse();
    }

    void OnDisable()
    {
        btnOpenPanel.GetComponent<TweenPosition>().PlayForward();
    }

	// Use this for initialization
	void Start () {

        Init();
	}
	
    public void SetPlayerName(string name)
    {
        lblName.text = name;
    }

    void Init()
    {
        SetPlayerName(TNet.TNManager.playerName);
        lblMoney.text = "0";
        PlayerChosen.value = false;
        ResultChosen.value = false;

        UpdatePlayerList();
        PlayerList.GetComponentInChildren<UILabel>().text = "Choose Player";
        btnPlayerList = PlayerList.GetComponentInChildren<UIButton>();
        
        PlayerChoice.items = PlanningPoker.instance.pokerChoices;
        PlayerChoice.value = "";
        PlayerChoice.GetComponentInChildren<UILabel>().text = "?";
        btnPlayerChoice = PlayerChoice.GetComponentInChildren<UIButton>();

        FinalResult.items = PlanningPoker.instance.pokerChoices;
        FinalResult.value = "";
        FinalResult.GetComponentInChildren<UILabel>().text = "?";
        btnFinalResult = FinalResult.GetComponentInChildren<UIButton>();
    }

    public void UpdatePlayerList()
    {
        PlayerList.items = GetParticipantNames();
    }

    public List<string> GetParticipantNames()
    {
        var list = new List<string>();
        var gm = PlanningPoker.instance;
        foreach (var p in gm.participants)
        {
            TNet.Player player = TNet.TNManager.GetPlayer(p.tno.ownerID);
            if (player != null) // could happen if somebody exits .  should look at a way to handle that
                list.Add(player.name);
        }
        return list;
    }


    public void ShowBetResults()
    {
        var o = Observer.instance;
        lblMoney.text = SideBetManager.instance.CheckBalance(o.tno.ownerID).ToString(); // make fancy effect
        SetBetResult(o.winMsg);
        o.winMsg = "NoBets";
    }

    public void HidePayoutRules()
    {
        payoutPanel.gameObject.SetActive(false);
    }

    public void ShowPayoutRules()
    {
        payoutPanel.gameObject.SetActive(true);
        lblPayoutText.text = @"We are now using the SMP Method for fair bet splitting.
If only one person wins, then that person gets all the pot.
If more than one person wins, the pot is split based on a the percentages of the winners' bets.
Long story short, if you bet low, then you get a low split.";
    }

    private void SetBetResult(string winMsg)
    {
        winMsg = winMsg.TrimEnd();
        string[] loseMessages = new string[] {
            "Sorry, you backed the wrong horse pardner.",
            "Well, there's still time to get a second mortgage.",
            "Wonder how much is in Viv's piggy bank...",
            "Keep trying, you're bound to win next time!",
            "A loser is only a winner that gave up too soon!",
            "You have to bet to win!",
            "I think you almost have this figured out.  Next time you're going to win big!"
        };

        if (winMsg == "NoBets")
        {
            lblBetResult.text = "No bets made this round.";
        }
        else if (!string.IsNullOrEmpty(winMsg))
        {
            winMsg = winMsg.Replace("NoBets",""); // get rid of the nobets string
            lblBetResult.text = winMsg;
        }
        else
        {
            int r = Random.Range(0, loseMessages.Length);
            lblBetResult.text = loseMessages[r];
        }
    }

	// Update is called once per frame
	void Update () {

        if (Observer.instance == null)
        {
            this.enabled = false;
            return;
        }

        bool enablePlayerChoices = true;
        var lbl = PlayerList.GetComponentInChildren<UILabel>();
        if (PlayerList.items.Count == 0)
            enablePlayerChoices = false;
        else
        {
            if (string.IsNullOrEmpty(PlayerList.value))
            {
                lbl.text = "Choose Player";
            }
        }
        bool pEnabled = true;
        bool fEnabled = true;
        if (!PlayerChosen.value || !enablePlayerChoices)
            pEnabled = false;

        if (!ResultChosen.value)
            fEnabled = false;

        btnPlayerList.isEnabled = pEnabled;
        btnPlayerChoice.isEnabled = pEnabled;

        btnFinalResult.isEnabled = fEnabled;

        bool betEnabled = false;

        int betAmount = GetIntFromString(BetAmount.value);

        if (((PlayerChosen.value && !string.IsNullOrEmpty(PlayerList.value) && !string.IsNullOrEmpty(PlayerChoice.value))
                    || (ResultChosen.value && !string.IsNullOrEmpty(FinalResult.value)))
                    && betAmount > 0)
        {
            betEnabled = true;
        }

        btnMakeBet.isEnabled = betEnabled;

        lblMoney.text = SideBetManager.instance.CheckBalance(Observer.instance.tno.ownerID).ToString(); // make fancy effect
       UpdateCurrentAction();
        UpdateBetHistory();
    }

    private int GetIntFromString(string s)
    {
        int result = 0;
        if (!string.IsNullOrEmpty(s))
        {
            if (int.TryParse(s, out result))
            {
                // maybe no if needed
            }
        }
        return result;
    }

    public void ValidateBetAmount()
    {
        int amount = GetIntFromString(BetAmount.value);
        int balance = GetIntFromString(lblMoney.text);
        if (amount <= 0 || amount > balance)
        {
            BetAmount.value = "0"; // set to 0 if they put in bad stuff
        }
    }

    public void UpdateCurrentAction()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Observer o in PlanningPoker.instance.observers)
        {
            sb.Append(SideBetManager.instance.GetCurrentAction(o));
        }
        lblCurrentHistory.text = sb.ToString();
    }

    public void UpdateBetHistory()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Observer o in PlanningPoker.instance.observers)
        {
            sb.Append(SideBetManager.instance.GetBetHistoryString(o));
        }
        lblBetHistory.text = sb.ToString();

    }

    public void ShowSideBets()
    {
        ShowOrHideSideBets(true);
    }

    public void HideSideBets()
    {
        ShowOrHideSideBets(false);
    }

    public void ShowOrHideSideBets(bool show)
    {
        if (show)
        {
            sideBetPosTween.PlayForward();
            sideBetCameraTween.PlayForward();
            mCurrentCameraTarget = cameraLook.target;
            cameraLook.target = sideBetPos;
            btnOpenPanel.GetComponent<TweenPosition>().PlayForward();
        }
        else
        {
            sideBetPosTween.PlayReverse();
            sideBetCameraTween.PlayReverse();
            cameraLook.target = mCurrentCameraTarget;
            btnOpenPanel.GetComponent<TweenPosition>().PlayReverse();
        }
    }

    public void OnMakeBet()
    {
        ValidateBetAmount();
        SideBet bet = null;
        if (PlayerChosen.value)
        {
            var participant = (from Participant p in PlanningPoker.instance.participants
                               where p.nameLabel.text == PlayerList.value
                               select p).FirstOrDefault();
            if (participant != null)
            {
                bet = new SideBet(SideBetType.PlayerChoice, participant.tno.ownerID.ToString(), PlayerChoice.value, BetAmount.value, false);
            }
        }
        else if (ResultChosen.value)
        {
            bet = new SideBet(SideBetType.FinalResult, null, FinalResult.value, BetAmount.value, false);
        }

        if (bet != null)
        {
            Observer.instance.winMsg = "";
            SideBetManager.instance.tno.Send("AddSideBet", TNet.Target.All, Observer.instance.tno.ownerID, (int)bet.betType, bet.playerID, bet.expectedValue, bet.betAmount, bet.won);
        }
    }

    public void CloseBetting()
    {
        btnMakeBet.isEnabled = false;
    }

    public void OpenBetting()
    {
        btnMakeBet.isEnabled = true;
    }
}
