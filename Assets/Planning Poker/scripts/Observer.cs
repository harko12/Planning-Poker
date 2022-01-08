using UnityEngine;
using System.Collections;
using TNet;
using System.Text;

public class ObserverValues
{
    public string name;
    public int? money;
    public float? winLossRatio;
    public int wins;
    public int losses;
    public int netWinnings;

    public ObserverValues()
    {

    }

    public ObserverValues(string n, int? m, float? w)
    {
        name = n;
        money = m;
        winLossRatio = w;
    }
}

public enum SideBetType
{
    PlayerChoice, FinalResult
}

public class SideBet
{
    public SideBetType betType;
    public string playerID;
    public string expectedValue;
    public string betAmount;
    public float betPercentage;
    public string wonAmount;
    public bool won;

    public SideBet()
    {

    }

    public SideBet(SideBetType t, string n, string e, string a, bool w)
    {
        betType = t;
        playerID = n;
        expectedValue = e;
        betAmount = a;
        won = w;
    }
}

public class Observer : TNBehaviour {

    protected Transform myTransform;
    private SideBetPanel mySideBet;
//    public System.Collections.Generic.List<SideBet> bets;
//    public System.Collections.Generic.List<SideBet> bettingHistory;
    public static Observer instance;
    private ObserverValues mValues;
    public ObserverValues values
    {
        get
        {
            return mValues;
        }
        set
        {
            tno.Send(200, Target.AllSaved, value.name, value.money, value.winLossRatio);
        }
    }
    void Awake()
    {
        if (tno.isMine)
            instance = this;
    }

    private void OnEnable()
    {
        TNManager.onPlayerJoin += OnNetworkPlayerJoin;
    }

    private void OnDisable()
    {
        TNManager.onPlayerJoin -= OnNetworkPlayerJoin;
    }

    void OnNetworkPlayerJoin(int channelID, Player p)
    {
        tno.Send(200, p, values.name, values.money, values.winLossRatio);
    }

    /*
    [RFC(201)]
    public void AddSideBet(int t, string n, string e, string a, bool w)
    {
        bets.Add(new SideBet((SideBetType)t, n, e, a, w));
        SideBetManager.instance.tno.Send("OnAddSideBet", Target.All, TNManager.playerID, t, int.Parse(a));
    }

    [RFC(202)]
    public void ModifySideBet(int index, int t, string n, string e, string a, bool w)
    {
        if (bets.Count > index && bets[index] != null)
            bets[index] = new SideBet((SideBetType)t, n, e, a, w);
    }

    [RFC(203)]
    public void DeleteSideBet(int index)
    {
        if (bets.Count > index && bets[index] != null)
            bets.RemoveAt(index);
    }
    */
    [RFC(204)]
    public void BettingRoundCompleted()
    {
    }
    
    // Use this for initialization
	void Start () {
        myTransform = transform;
        mySideBet = SideBetPanel.instance;
        var pp = PlanningPoker.instance;
        gameObject.name = "Observer " + (Observer.instance == this ? "(me)" : "(them)");
        PlanningPokerTools.MakeGameObjectChildOf(gameObject, pp.gameObject);
        var partPanel = NGUITools.FindActive<UITable>()[0];// PlanningPoker.GetInstance().participantPanel;
        partPanel.repositionNow = true;
        InitForHand();
        // register ourselves with the pokermanager
        pp.AddObserver(this);
        pp.EnableSideBetPanel(true);
	}

    void OnDestroy()
    {
        if (SideBetManager.instance != null)
        {
            SideBetManager.instance.CloseAccount(this);
        }
        PlanningPoker.instance.RemoveObserver(this);
    }
    private void InitForHand()
    {
        if (tno.isMine)
        {
            if (Observer.instance != null)
            {
                mySideBet.SetPlayerName(TNManager.playerName);
                int startMoney = SideBetManager.instance.OpenAccount(this);
                Observer.instance.values = new ObserverValues(TNManager.playerName, startMoney, 0f);
                Observer.instance.winMsg = "NoBets";
            }
        }
    }

    [RFC(200)]
    public void OnSetObserverValues(string n, int? m, float? w)
    {
        if (mValues == null) mValues = new ObserverValues();
        if (n != null) mValues.name = n;
        if (m != null) mValues.money = m;
        if (w != null) mValues.winLossRatio = w;
    }

    [RFC]
    public void OnAddWinOrLoss(bool isWin, int amount)
    {
        if (isWin)
        {
            mValues.wins++;
            mValues.netWinnings += amount;
        }
        else
        {
            mValues.losses++;
            mValues.netWinnings -= amount;
        }
    }
    public string winMsg = "";

    [RFC]
    public void OnAddWinMsg(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        {
            msg = "";
        }
        else
        {
            winMsg += msg + "\n";
        }
    }

}
