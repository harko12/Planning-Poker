using UnityEngine;
using System.Collections;
using sg = System.Collections.Generic;
using System.Linq;
using TNet;
using System.Text;

public class PlanningPoker : MonoBehaviour{

    public static PlanningPoker instance;
    public PlanningPokerPager Pager;
    public GameObject dealerPrefab;
    public GameObject participantPrefab;
    public GameObject observerPrefab;
    public GameObject timerPrefab;

    public UIRoot Root;

    public SideBetPanel sideBetPanel;

    public LookAtTarget cameraLook;

    public Transform lookAwayTarget;

    public GameObject mainPanel;
    public GameObject menuPanel;
    public GameObject holdingPanel;
    public UIElementPivot elementPivot;

    public System.Collections.Generic.List<string> pokerChoices;

	// Use this for initialization
	void Awake () {
        var guiCamera = NGUITools.FindCameraForLayer(mainPanel.layer);
        cameraLook = guiCamera.GetComponent<LookAtTarget>();
        instance = this;
	}
	
    void Start()
    {
        ShowMenu();
    }

    public void EnableSideBetPanel(bool enabled)
    {
        sideBetPanel.enabled = enabled;
    }

    void OnNetworkPlayerRenamed(Player p, string previous)
    {
        if (p.id == TNManager.playerID)
        {
            if (Participant.instance != null) // could be, if observing
            {
                Participant.instance.values = new ParticipantValues(p.name, null, null);
            }

            if (p.id == Dealer.instance.dealerId)
            {
                Dealer.instance.values = new ParticipantValues(p.name, null, null);
            }

            if (Observer.instance != null)
            {
                Observer.instance.values = new ObserverValues(p.name, null, null);
            }
        }

        UpdatePlayerLists();
    }

    void OnNetworkPlayerLeave(Player p)
    {
        //participantPanel.repositionNow = true;
        UpdatePlayerLists();
    }

    [HideInInspector]
    public sg.List<Participant> participants = new sg.List<Participant>();
    [HideInInspector]
    public sg.List<Observer> observers = new sg.List<Observer>();

    public void SpawnParticipant(bool Observing)
    {
        Participant p = Participant.instance;
        Observer o = Observer.instance;
        if (!Observing)
        {
            if (o != null)
            {
                // find a way to preserve stats/money
                o.tno.DestroySelf();
            }
            if (p == null)
                TNManager.Create(participantPrefab, transform.position, Quaternion.identity, false);
        }
        else
        {
            if (o == null)
            {
                TNManager.Create(observerPrefab, false);
            }
            if (p != null)
                p.tno.DestroySelf();
        }
    }

    public void AddObserver(Observer o)
    {
        observers.Add(o);
    }

    public void RemoveObserver(Observer o)
    {
        observers.Remove(o);
    }

    public void UpdateSideBetPlayerList()
    {
        if (sideBetPanel.enabled)
        {
            sideBetPanel.UpdatePlayerList();
        }
    }

    public void AddParticipant(Participant p)
    {
        Pager.AddParticipant(p);
        participants.Add(p);
        UpdateSideBetPlayerList();
        if (handPlaying)
            p.tno.Send("OnHandStarted", TNManager.GetPlayer(p.tno.ownerID)); // let that person start the hand
    }

    public void RemoveParticipant(Participant p)
    {
        participants.Remove(p);
        UpdateSideBetPlayerList();
    }
    private bool handPlaying = false;
    public IEnumerator HandLoop()
    {
        handPlaying = true;
        bool undecided = false;
        yield return new WaitForSeconds(2f);  //give the participants time to prepare.  should probalby do this with a callback or something
        while(handPlaying)
        {
            undecided = false;
            float pctComplete = Timer.instance.PercentComplete;
            //Debug.Log("percent complete: " + pctComplete.ToString());
            if (pctComplete >= 1) // time is up
            {
                pctComplete = 1;
                handPlaying = false;
                continue;
            }
            foreach(var p in participants)
            {
                if (p != null)
                {
                    if (!p.valueChosen)
                    {
                        undecided = true;
                        break;
                    }
                }
            }

            if (!undecided)
            {
                handPlaying = false;
            }
            yield return new WaitForSeconds(1f);
        }
        HandStopped();
        yield return null;
    }

    public void UpdatePlayerLists()
    {
        participants = Root.GetComponentsInChildren<Participant>().Where(p => !(p is Dealer)).ToList();
        observers = gameObject.GetComponentsInChildren<Observer>().ToList<Observer>();
        Pager.UpdatePagePositions(participants);
        UpdateSideBetPlayerList();
    }

    public void HandStarted()
    {
        HandResult.instance.ResetResult();
        UpdatePlayerLists();

        foreach (var p in participants) p.tno.Send("OnHandStarted", Target.All);
        //foreach (var o in observers) o.tno.Send("OnHandStarted", Target.All);
        SideBetManager.instance.tno.Send("OnHandStarted", Target.All);

        Timer.instance.StartCountdown();
        StartCoroutine("HandLoop");
        Dealer.instance.goButton.isEnabled = false;
    }

    public string PlayerResults = "";
    public void HandStopped()
    {
        Timer.instance.StopCountdown();
        
        //foreach (var o in observers) o.tno.Send("OnHandStopped", Target.All);
        StringBuilder sb = new StringBuilder();
        var figureResults = new sg.Dictionary<string, float>();
        foreach (var p in participants)
        {
            var value = p.ActualValue ?? "";
            sb.AppendFormat("{0}:{1},", p.tno.ownerID, value);

            if (!string.IsNullOrEmpty(value))
            {
                if (!figureResults.ContainsKey(value))
                    figureResults.Add(value, 0);
                figureResults[value]++;
            }
            p.tno.Send("OnHandStopped", Target.All);
        }
        PlayerResults = sb.ToString().TrimEnd(',');
        HandResult.instance.HandResultString = GetHandResultString(figureResults);
        HandResult.instance.PlayerResultString = PlayerResults;
        HandResult.instance.RevealHand(figureResults.Keys.ToArray(), figureResults.Values.ToArray());
        Dealer.instance.goButton.isEnabled = true;

        SideBetManager.instance.tno.Send("OnHandStopped", Target.All);
    }

    private string GetHandResultString(sg.Dictionary<string,float> results)
    {
        StringBuilder sb = new StringBuilder();
        float lastValue = 0;
        foreach (var option in results.OrderByDescending(r => r.Value))
        {
            if (lastValue != 0 && lastValue > option.Value)
                continue; // skip because we already ahve the top choices
            lastValue = option.Value;
            sb.AppendFormat("{0}:", option.Key);
        }
        return sb.ToString().TrimEnd(':');
    }

    public void SetDealer(Player p)
    {
        Dealer.instance.SetPlayerAsDealer(p);
    }

    public void ShowMenu()
    {
        cameraLook.target = menuPanel.transform;
    }

    public void ShowPoker()
    {
        cameraLook.target = mainPanel.transform;
    }

    public void LookAway()
    {
        cameraLook.target = lookAwayTarget;
    }
}
