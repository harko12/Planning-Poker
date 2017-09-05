using UnityEngine;
using System.Collections;
using TNet;

public class GamePlayer : TNBehaviour {

    static public GamePlayer instance;

    Vector3 mTarget = Vector3.zero;

    public Vector3 target
    {
        set
        {
            tno.Send("OnSetTarget", Target.AllSaved, value);
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
    void Awake()
    {
        if (TNManager.isThisMyObject)
            instance = this;
    }
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.MoveTowards(transform.position, mTarget, 3f * Time.deltaTime);
	}

    [RFC]
    void OnSetTarget(Vector3 pos)
    {
        mTarget = pos;
    }

    void OnNetworkPlayerJoin(Player p)
    {
        tno.Send("OnSetTargetImmediate", p, transform.position);
    }
    
    [RFC]
    void OnSetTargetImmediate(Vector3 pos)
    {
        transform.position = pos;
    }
}
