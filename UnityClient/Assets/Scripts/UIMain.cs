using UnityEngine;
using System.Collections;

public class UIMain : MonoBehaviour {
    public UI PanelGame;
    public UGameEngine PanelLearn;
    public GameObject BtnLearn;
    public GameObject BtnPlay;

	// Use this for initialization
	void Start () {
        UIEventListener.Get(BtnLearn).onClick = OnClickLearn;
        UIEventListener.Get(BtnPlay).onClick = OnClickPlay;
	}

    void OnClickLearn(GameObject go)
    {
        PanelLearn.gameObject.SetActive(true);
        Close();
    }

    void OnClickPlay(GameObject go)
    {
        Close();
        PanelGame.gameObject.SetActive(true);
        PanelGame.Chessboard.gameObject.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void Close()
    {
        gameObject.SetActive(false);
    }
}
