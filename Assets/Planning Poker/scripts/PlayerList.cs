using UnityEngine;
using System.Collections;
using System.Linq;
using TNet;

public class PlayerList : MonoBehaviour {

    public UIScrollView scrollView;

    public UIGrid grid;
    /// <summary>
    /// interval, in seconds
    /// </summary>
    public float PollInterval;
    public GameObject PlayerLinePrefab;
	// Use this for initialization
	void Start () {
        if (PollInterval <= 0)
            PollInterval = .25f;

        StartCoroutine("RefreshList");
	}
	
    public IEnumerator RefreshList()
    {
        while(true)
        {
            UpdatePlayerList();
            yield return new WaitForSeconds(PollInterval);
        }
    }
    public void UpdatePlayerList()
    {
        var entries = grid.GetComponentsInChildren<PlayerListEntry>();
        var playerList = TNManager.players;
        playerList.Add(TNManager.GetPlayer(TNManager.playerID)); // add 'us'
        foreach (Player p in TNManager.players)
        {
            var foundEntry = (from PlayerListEntry ple in entries
                              where ple.myPlayer.id == p.id
                              select ple).FirstOrDefault();
            if (foundEntry != null)
            {
                foundEntry.UpdateLine(p);
                grid.repositionNow = true;
            }
            else
            {
                AddPlayerLine(p);
            }

        }

        //linq to get unused entries? var oldEntries = (from )
        // clear old player lines
        foreach (var entry in entries)
        {
            bool found = (from Player p in playerList.ToArray()
                               where p.id == entry.myPlayer.id
                               select p).Any();
            if (!found)
            {
                entry.transform.parent = null;
                Destroy(entry.gameObject);
            }

        }
    }

    private void AddPlayerLine(Player p)
    {
        var pl = NGUITools.AddChild(grid.gameObject, PlayerLinePrefab);
        var playerLine = pl.GetComponent<PlayerListEntry>();
        playerLine.myPlayer = p;
        playerLine.UpdateLine(p);
    }

    private void OnEnable()
    {
        TNManager.onRenamePlayer += OnNetworkPlayerRenamed;
        TNManager.onPlayerLeave += OnNetworkPlayerLeave;
        TNManager.onPlayerJoin += OnNetworkPlayerJoin;
    }

    private void OnDisable()
    {
        TNManager.onRenamePlayer -= OnNetworkPlayerRenamed;
        TNManager.onPlayerLeave -= OnNetworkPlayerLeave;
        TNManager.onPlayerJoin -= OnNetworkPlayerJoin;
    }

    void OnNetworkPlayerJoin(int channelID, Player p)
    {
        UpdatePlayerList();
    }

    void OnNetworkPlayerLeave(int channelID, Player p)
    {
        UpdatePlayerList();
    }

    void OnNetworkPlayerRenamed(Player p, string previous)
    {
        UpdatePlayerList();
    }
}
