using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TNet;
using System.Text;

public class SideBetManager : TNBehaviour {

    public static SideBetManager instance;
    public int BankStartupCash = 10000;
    private Dictionary<int, int> Bank;
    private Dictionary<int, int> mPots; // dictionary of SideBetType(int), pot value
    private Dictionary<int, System.Collections.Generic.List<SideBet>> bets;
    public System.Collections.Generic.List<SideBet> GetBetsForObserver(int observerId)
    {
        if (!bets.ContainsKey(observerId))
            bets.Add(observerId, new System.Collections.Generic.List<SideBet>());

        if (bets[observerId] == null)
        {
            bets[observerId] = new System.Collections.Generic.List<SideBet>();
        }

        return bets[observerId];
    }

    void Awake()
    {
        instance = this;
    }
    // Use this for initialization
	void Start () {
        mPots = new Dictionary<int, int>();
        Bank = new Dictionary<int, int>();
        bets = new Dictionary<int, System.Collections.Generic.List<SideBet>>();
        //tno.Send("ResetForNextRound", Target.All);
        ResetForNextRound();
    }

    private void OnEnable()
    {
        TNManager.onPlayerJoin += OnNetworkPlayerJoin;
    }

    private void OnDisable()
    {
        TNManager.onPlayerJoin -= OnNetworkPlayerJoin;
    }

    public void OnNetworkPlayerJoin(int channelID, Player p)
    {
    }

    [RFC]
    public void AddSideBet(int observerId, int t, string n, string e, string a, bool w)
    {
        var betsList = GetBetsForObserver(observerId);
        bool hasBetType = (from SideBet b in betsList where (int)(b.betType) == t select b).Any();
        if (!hasBetType)
        {
            betsList.Add(new SideBet((SideBetType)t, n, e, a, w));
            OnAddSideBet(observerId, t, int.Parse(a));
        }

    }


    [RFC]
    public void OnAddSideBet(int observerId, int betType, int amount)
    {
        if (mPots.ContainsKey(betType))
            mPots[betType] += amount;
        if (Bank.ContainsKey(observerId))
            Bank[observerId] -= amount;
    }

    [RFC]
    public void OnOpenAccount(int observerId, int moneyAmount)
    {
        if (!Bank.ContainsKey(observerId))
            Bank.Add(observerId, 0);

        Bank[observerId] = moneyAmount;
    }

    [RFC]
    public void OnCloseAccount(int observerId)
    {
        Bank.Remove(observerId);
    }

    [RFC]
    public void AdjustAccount(int observerId, int amount)
    {
        if (Bank.ContainsKey(observerId))
            Bank[observerId] += amount;
    }

    public int CheckBalance(int observerId)
    {
        if (!Bank.ContainsKey(observerId))
        {
            Debug.LogWarning("No Bank account for player " + observerId);
            return 0;
        }
        return Bank[observerId];
    }

    public int OpenAccount(Observer o)
    {
        tno.Send("OnOpenAccount",Target.AllSaved, o.tno.ownerID, BankStartupCash);
        return BankStartupCash;
    }

    public void CloseAccount(Observer o)
    {
        tno.Send("OnCloseAccount", Target.AllSaved, o.tno.ownerID);
    }

    public void OnStartRound()
    {
        if (TNManager.isHosting)
        {
            /*
            foreach (Observer o in PlanningPoker.instance.observers)
            {
                o.tno.Send("OnStartRound", TNet.Target.All);
            }
             */
            BettingActive = true;
            //StartCoroutine("BettingRound");
        }
    }

    void OnGUI()
    {
        return; // no messages right now
        /*
        if (TNManager.isHosting)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Bank Status:");
            foreach (var k in Bank.Keys)
            {
                sb.AppendFormat("Bank[{0}] balance: {1}\n:", k, Bank[k]);
            }

            GUI.Label(new Rect(0,200, 200, 200), sb.ToString());
        }
         */
    }
    private bool BettingActive = false;

    [RFC]
    public void OnHandStopped()
    {
        OnStopRound();
    }

    [RFC]
    public void OnHandStarted()
    {
        OnStartRound();
    }

    public void OnStopRound()
    {
        if (!TNManager.isHosting) return;
        /*
        foreach (Observer o in PlanningPoker.instance.observers)
        {
            o.tno.Send("OnStopRound", TNet.Target.All);
        }
         */

        BettingActive = false;
        var winners = new Dictionary<int, System.Collections.Generic.List<SideBet>>();
        // tally the bets

        // first store the results
        var results = new Dictionary<string, string>();
        foreach (var pResult in HandResult.instance.PlayerResultString.Split(','))
        {
            if (string.IsNullOrEmpty(pResult))
            {
                Debug.LogWarning("Empty PlayerResults");
                continue;
            }
            string[] blah = pResult.Split(':');
            results[blah[0]] = blah[1];
        }

        var finalResult = HandResult.instance.HandResultString;

        var fResults = new System.Collections.Generic.List<string>(finalResult.Split(':'));

        // check each bet to see who won, while adding up all the pot money
        foreach (Observer o in PlanningPoker.instance.observers)
        {
            var betList = GetBetsForObserver(o.tno.ownerID);
            for (int i = 0; i < betList.Count; i++)
            {
                var b = betList[i];
                var bType = (int)b.betType;
                if (!winners.ContainsKey(bType))
                {
                    winners.Add(bType, new System.Collections.Generic.List<SideBet>());
                }
                // set percentage of bet
                var pot = mPots[bType];
                b.betPercentage = (float.Parse(b.betAmount) / pot);
                bool won = false;
                switch (b.betType)
                {
                    case SideBetType.FinalResult:
                        if (fResults.Contains(b.expectedValue))
                        {
                            won = true;
                        }
                        break;
                    case SideBetType.PlayerChoice:
                        if (results.ContainsKey(b.playerID) && results[b.playerID] == b.expectedValue)
                        {
                            won = true;
                        }
                        break;
                }
                if (won)
                {
                    winners[bType].Add(b);
                }
                SetBetWon(i, won, o.tno.ownerID);
//                tno.Send("SetBetWon", Target.All, i, won, o.tno.ownerID);
            }
        }
        /*
        // now figure out the payouts
        int key = (int)SideBetType.PlayerChoice;
        int byPlayerPayout = 0;
        int byFinalResultPayout = 0;

        var adjustedPayouts = new Dictionary<int, int>();
        adjustedPayouts.Add(key, 0);
        if (winners.ContainsKey(key) && winners[key].Count > 0)
        {
            byPlayerPayout = mPots[key] / winners[key].Count;
            adjustedPayouts[key] = byPlayerPayout;
        }

        key = (int)SideBetType.FinalResult;
        adjustedPayouts.Add(key, 0);
        if (winners.ContainsKey(key) && winners[key].Count > 0)
        {
            byFinalResultPayout = mPots[key] / winners[key].Count;
            adjustedPayouts[key] = byFinalResultPayout;
        }
        */
        // hand out results and money
        foreach (Observer o in PlanningPoker.instance.observers)
        {
            var betList = GetBetsForObserver(o.tno.ownerID);
            for (int i = 0; i < betList.Count; i++)
            {
                var b = betList[i];
                var bType = (int)b.betType;
                //int payout = adjustedPayouts[bType];
                int payout = GetSMPPayout(b, bType, winners);
                string wonMsg = "{0}";
                if (b.won)
                {
                    switch (b.betType)
                    {
                        case SideBetType.FinalResult:
                            wonMsg = "You won ${0} betting on the end result!";
                            break;
                        case SideBetType.PlayerChoice:
                            wonMsg = "You won ${0} betting on a player choice!";
                            break;
                    }
                    tno.Send("AdjustAccount", Target.AllSaved, o.tno.ownerID, payout);
                    var winMsg = string.Format(wonMsg, payout);
                    tno.Send("SetBetPayout", Target.All, bType, payout, o.tno.ownerID);
                    o.tno.Send("OnAddWinMsg", TNManager.GetPlayer(o.tno.ownerID), winMsg);
                }
                o.tno.Send("OnAddWinOrLoss", Target.AllSaved, b.won, (b.won ? payout : int.Parse(b.betAmount)));
            }
        }
        tno.Send("ResetForNextRound", Target.All);

        foreach (var o in PlanningPoker.instance.observers)
        {
            o.tno.Send("BettingRoundCompleted", Target.All);
        }

        tno.Send("ShowBetResults", Target.All);
    }

    private int GetSMPPayout(SideBet bet, int key, Dictionary<int, System.Collections.Generic.List<SideBet>> winners)
    {
        int payout = 0;
        if (winners.ContainsKey(key) && winners[key].Count > 0)
        {
            float winnerBetsPercentage = winners[key].Sum(b => b.betPercentage);
            float myPercentage = bet.betPercentage * (1 / winnerBetsPercentage); // SMP's businessrule.  you get only the percentage that you put in, when the pot needs to be split
            payout = (int)(myPercentage * mPots[key]);
            //Debug.LogFormat("Bet {0}$ (%{1}, win% {2}, my% {3} Payout {4}$", bet.betAmount, bet.betPercentage, winnerBetsPercentage,  myPercentage, payout);
        }
        return payout;
    }

    [RFC]
    public void SetBetWon(int betIndex, bool won,int ownerId)
    {
        var bets = GetBetsForObserver(ownerId);
        bets[betIndex].won = won;
    }

    [RFC]
    public void SetBetPayout(int betType, int payout, int ownerId)
    {
        var bets = GetBetsForObserver(ownerId);
        foreach (var b in bets)
        {
            if (b.betType == (SideBetType)betType)
            {
                b.wonAmount = payout.ToString();
            }
        }
    }

    [RFC]
    public void ShowBetResults()
    {
        if (PlanningPoker.instance.sideBetPanel.enabled)
        {
            PlanningPoker.instance.sideBetPanel.ShowBetResults();
        }
    }

    [RFC]
    public void ResetForNextRound()
    {
        foreach (var betType in System.Enum.GetValues(typeof(SideBetType)))
        {
            int t = (int)betType;
            mPots[t] = 0;
        }

        foreach (int observer in bets.Keys)
        {
            var betsList = GetBetsForObserver(observer);
            //GetBettingHistoryForObserver(observer).AddRange(GetBetsForObserver(observer)); // add bets to history
            betsList.Clear();
        }
        
    }

    public IEnumerable BettingRound()
    {
        while(BettingActive)
        {
            yield return null;
        }

        yield return null;
    }

    public string GetCurrentAction(Observer o)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (o != null)
        {
            var betsList = GetBetsForObserver(o.tno.ownerID);
            if (betsList.Count > 0)
            {
                sb.AppendLine(TNManager.GetPlayer(o.tno.ownerID).name + " is betting");
                foreach (SideBet b in betsList)
                {
                    if (b.betType == SideBetType.PlayerChoice)
                    {
                        int playerId;
                        if (int.TryParse(b.playerID, out playerId))
                        {
                            Player p = TNManager.GetPlayer(playerId);
                            if (p != null)
                                sb.AppendLine(string.Format("${0} that {1} will choose {2}", b.betAmount, p.name, b.expectedValue));
                        }
                    }
                    else if (b.betType == SideBetType.FinalResult)
                        sb.AppendLine(string.Format("${0} that the final result will be {1}", b.betAmount, b.expectedValue));
                }
            }

        }
        return sb.ToString();
    }

    public string GetBetHistoryString(Observer o)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        if (o != null && o.values != null && TNManager.GetPlayer(o.tno.ownerID) != null)
        {
            sb.AppendLine(string.Format("{0} W: {1} L: {2} net: ${3}", TNManager.GetPlayer(o.tno.ownerID).name, o.values.wins, o.values.losses, o.values.netWinnings));
        }
        return sb.ToString();
    }


}
