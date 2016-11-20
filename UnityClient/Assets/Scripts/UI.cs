using UnityEngine;
using System.Collections;

public class UI : MonoBehaviour {
    public GameObject spriteGameOver;
    public GameObject btnUndo;
    public UILabel labelWinner;
    public UILabel labelTime;
    public UILabel labelTun;

    // Use this for initialization
    void Start () {
        UIEventListener.Get(spriteGameOver).onClick = OnClickGameOver;
        UIEventListener.Get(btnUndo).onClick = OnClickUndo;
        spriteGameOver.SetActive(false);
        UChessboard.Instance.onGameOver = OnGameOver;
	}

    void OnGameOver(ECampType Winner)
    {
        spriteGameOver.SetActive(true);
        if(Winner == UPlayer.LocalPlayer.Camp)
        {
            labelWinner.text = "你赢了";
        }
        else
        {
            labelWinner.text = "你输了";
        }
    }

    void OnClickGameOver(GameObject go)
    {
        UChessboard.Instance.Restart();
        spriteGameOver.SetActive(false);
    }

    void OnClickUndo(GameObject go)
    {
        UChessboard.Instance.Undo();
        UChessboard.Instance.Undo();
    }
	
	// Update is called once per frame
	void Update () {
        labelTime.text =string.Format("{0:D2}:{1:D2}", (int)(UChessboard.Instance.game_start_timer/60), (int)(UChessboard.Instance.game_start_timer%60));
        labelTun.text = UChessboard.Instance.NowPlayer== ECampType.Red?"[ff0000]红方":"[000000]黑方";
    }
}
