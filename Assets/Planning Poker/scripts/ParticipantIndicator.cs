using UnityEngine;
using System.Collections;
using TNet;

public class ParticipantIndicator : MonoBehaviour {
    public UISprite background;

    private Participant myParticipant;
	// Use this for initialization
	void Start () {
        myParticipant = GetComponentInParent<Participant>();
        if (myParticipant != null)
        {
            if (TNManager.playerID == myParticipant.tno.ownerID) // if we are the participant..
                background.enabled = true;

        }
	}
}
