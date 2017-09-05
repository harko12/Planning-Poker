using UnityEngine;
using System.Collections;
using TNet;

public class Timer : TNBehaviour {
    public static Timer instance;

    public UILabel secsLabel;
    public UILabel tenthsLabel;
    public UILabel timerLabel;
    public UISprite timerLabelBg;

    public UIInput secondsInput;
    public int GameSeconds = 60;
    public void SetGameSeconds()
    {
        int v = 0;
        if (!int.TryParse(secondsInput.value, out v) || v <= 0)
            v = GameSeconds;

        tno.Send("OnSetGameSeconds", Target.AllSaved, v);
        tno.Send("OnSetTimeLabels", Target.All, v);
    }

    [RFC]
    public void OnSetGameSeconds(int secs)
    {
        GameSeconds = secs;
        Dealer.instance.gameSecondsInput.value = secs.ToString();
    }

    private Color spriteStart;
    private UITweener flash;
    void Awake()
    {
        instance = this;
    }
	// Use this for initialization
	void Start () {
        spriteStart = timerLabelBg.color;
        flash = timerLabelBg.GetComponent<TweenColor>();
        var timerPos = GameObject.Find("TimerPosition") as GameObject;
        PlanningPokerTools.MakeGameObjectChildOf(gameObject, timerPos);
        SetGameSeconds();
	}
    public void StartCountdown()
    {
        PercentComplete = 0;
        SetGameSeconds();
        tno.Send("OnCountdownStart", Target.AllSaved);
        if (TNManager.playerID == Dealer.instance.dealerId)
            StartCoroutine("Countdown");

    }

    public void PauseCountdown()
    {
        isPaused = !isPaused;

        flash.enabled = !isPaused;
    }

    public void StopCountdown()
    {
        forceStop = true;
    }

    public bool isPaused = false;
    public bool forceStop = false;
    public IEnumerator Countdown()
    {
        float startTime = Time.time;
        float refTime = startTime;
        float lastTime = startTime;
        float restSeconds = 0;
        //int roundedSeconds = 0;
        bool quit = forceStop = false;
        float hpMaintInterval = 100;
        float hpMaintCounter = 0;
        while (!quit && !forceStop)
        {
            if (hpMaintCounter > hpMaintInterval)
            {
                hpMaintCounter = 0;
            }
            float now = Time.time;
            // update time
            if (isPaused)
            {
                refTime += (now - lastTime); 
                // keep refTime updating along with Time, so that it doesnt see
                // that time is passing
            }
            else
            {
                var guiTime = now - refTime;
                restSeconds = GameSeconds - guiTime;
                tno.Send("OnSetTimeLabels", Target.All, restSeconds);
                if (restSeconds <= 0)
                {
                    quit = true;
                }
            }
            lastTime = now;
            // update counters
            hpMaintCounter++;
            yield return null; // new WaitForSeconds(1);
        }
        tno.Send("OnCountdownFinished", Target.AllSaved);
        yield return null;
    }
    public float PercentComplete;

    [RFC]
    public void OnSetTimeLabels(float s)
    {
        SetTimeLabels(s);
    }
    private void SetTimeLabels(float seconds)
    {
        PercentComplete = ((float)GameSeconds - seconds) / (float)GameSeconds;
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        
        int mins = t.Minutes;
        int secs = t.Seconds;
        int milliSecs = t.Milliseconds;

        secsLabel.text = string.Format("{0:0}:{1:00}", mins, secs);
        tenthsLabel.text = string.Format("{0:00}", milliSecs);
    }
    public string StoppedText = "Awaiting Deal";
    public string RunningText = "Make a Play!";
    [RFC]
    public void OnCountdownStart()
    {
        flash.enabled = true;
        timerLabel.text = RunningText;
    }

    [RFC]
    public void OnCountdownFinished()
    {
        InitTimer();
    }

    /// <summary>
    /// prepare the timer to run again.  more of a reset, than init.. but not really
    /// </summary>
    private void InitTimer()
    {
        flash.enabled = false;
        timerLabelBg.color = spriteStart;
        timerLabel.text = StoppedText;
        SetTimeLabels(GameSeconds);
        PercentComplete = 1;
    }

}
