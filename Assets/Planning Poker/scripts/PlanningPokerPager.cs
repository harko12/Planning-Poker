using System.Collections;
using sg = System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlanningPokerPager : MonoBehaviour {

    public UIButton PrevButton, NextButton;
    public UILabel StatusLabel;

    public UIRoot Root;
//    public UIPanel ParticipantsPanel;
//    public Transform tablePosition;
    public GameObject pagePanelPrefab;

    public int MaxPerPage;

    private sg.List<UIPanel> mPages;

    private int mCurrentPage;

    private void Awake()
    {
        mPages = new sg.List<UIPanel>();
        mCurrentPage = 0;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private UIPanel GetCurrentPage()
    {
        if (!mPages.Any() || mPages.Count < mCurrentPage + 1)
        {
            return AddPage();
        }
        else
        {
            return mPages[mCurrentPage];
        }
    }

    private UIPanel GetCurrentPageForNewParticipant()
    {
        var page = GetCurrentPage();
        var table = page.GetComponentInChildren<UITable>();
        if (table.transform.childCount >= MaxPerPage)
        {
            mCurrentPage++;
            page = GetCurrentPage(); // make a new page
        }
        return page;
    }

    private UIPanel AddPage()
    {
        Debug.Log(string.Format("Adding page {0}", mCurrentPage));
        var go = Instantiate(pagePanelPrefab, new Vector3(0,0,0), Quaternion.identity) as GameObject;
        var pagePanel = go.GetComponent<UIPanel>();
        PlanningPokerTools.MakeGameObjectChildOf(go, Root.gameObject);
        mPages.Add(pagePanel);

        SetPage(mCurrentPage);
        pagePanel.name = string.Format("Participant Page {0}", mCurrentPage);

        return pagePanel;
    }
    public void PrevPage()
    {
        mCurrentPage = mCurrentPage == 0 ? 0 : --mCurrentPage;
        SetPage(mCurrentPage);
    }

    public void NextPage()
    {
        mCurrentPage = mCurrentPage == mPages.Count ? mPages.Count : ++mCurrentPage;
        SetPage(mCurrentPage);
    }

    public void SetPage(int pageIndex)
    {
        for (int lcv=0; lcv < mPages.Count; lcv++)
        {
            var page = mPages[lcv];
            if (lcv == pageIndex)
            {
                page.alpha = 1f;
//                NGUITools.SetActive(page.gameObject, true);
            }
            else
            {
                page.alpha = 0f;
//                NGUITools.SetActive(page.gameObject, false);
            }
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        var statusText = string.Format("Page {0} of {1}", mCurrentPage + 1, mPages.Count);
        bool showPrev = true, showNext = true;
        if (mCurrentPage == 0)
        {
            showPrev = false;
        }
        else if (mCurrentPage == mPages.Count - 1)
        {
            showNext = false;
        }

        if (mPages.Count <= 1)
        {
            showNext = false;
            showPrev = false;
            statusText = "";
        }


        NGUITools.SetActive(PrevButton.gameObject, showPrev);
        PrevButton.isEnabled = showPrev;
        NGUITools.SetActive(NextButton.gameObject, showNext);
        NextButton.isEnabled = showNext;

        StatusLabel.text = statusText;
    }

    public void AddParticipant(Participant p)
    {
        var partPanel = GetCurrentPageForNewParticipant();
        var table = partPanel.GetComponentInChildren<UITable>();
        //var partPanel = NGUITools.FindActive<UITable>()[0];// PlanningPoker.GetInstance().participantPanel;
        p.gameObject.name = "Participant " + (Participant.instance == p ? "(me)" : "(them)");
        PlanningPokerTools.MakeGameObjectChildOf(p.gameObject, table.gameObject);
        table.repositionNow = true;
    }

    public void UpdatePagePositions(sg.List<Participant> pList)
    {
        // clean slate
        var holding = PlanningPoker.instance.holdingPanel;
        foreach (Participant p in pList)
        {
            Debug.LogFormat("moving {0} to holding", p.GetInstanceID());
            PlanningPokerTools.MakeGameObjectChildOf(p.gameObject, holding.gameObject);
            p.gameObject.transform.position = Vector3.zero;
        }

        UpdateUI();

        // re-add remaining
        int currentPage = 0;
        int currPageCount = 0;
        UITable table = null;
        bool changePage = false;
        var playerPage = 0;
        foreach (Participant p in pList)
        {
            if (currPageCount >= MaxPerPage)
            {
                changePage = true;
                if (currentPage == mPages.Count - 1)
                {
                    AddPage();
                }
                currentPage++;
                currPageCount = 0;
            }

            if (table == null || changePage)
            {
                table = mPages[currentPage].GetComponentInChildren<UITable>();
            }
            PlanningPokerTools.MakeGameObjectChildOf(p.gameObject, table.gameObject);
            Debug.LogFormat("Adding {0}-{2} to page {1}", p.GetInstanceID(), table.transform.parent.name, p.name);
            currPageCount++;
            table.repositionNow = true;
            if (p.tno.isMine)
            {
                playerPage = currentPage;
            }
            changePage = false;
        }
        mCurrentPage = playerPage;
        SetPage(playerPage);
    }
}
