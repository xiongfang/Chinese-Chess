using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 本地玩家控制器
/// </summary>
public class ULocalPlayerController : UPlayerController {

    enum EOpStep
    {
        None,   //无法操作
        Select, //选择棋子
        Push    //放下棋子
    }

    //操作阶段
    EOpStep MyOperateStep;
    //选择的棋子
    UChess selectedChess;

    ECampType ViewType;

    public override void Update()
    {
        //H键切换视图
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (ViewType == ECampType.Red)
                ViewType = ECampType.Black;
            else
                ViewType = ECampType.Red;
            UChessboard.Instance.SetPlayerView(ViewType);
        }

        switch (MyOperateStep)
        {

            case EOpStep.Select:
                {
                    //选择棋子
                    if (Input.GetMouseButtonDown(0))
                    {
                        RaycastHit HitInfo;
                        if (Physics.Raycast(UChessboard.Instance.GetViewCamera().ScreenPointToRay(Input.mousePosition), out HitInfo))
                        {
                            UChess HitChess = UChessboard.Instance[UChessboard.Instance.WorldToPos(HitInfo.point)];
                            if (HitChess != null && HitChess.campType == MyCamp)
                            {
                                selectedChess = HitChess;
                                MyOperateStep = EOpStep.Push;
                                ShowMoveTargets();
                            }
                        }
                    }
                }
                break;
            case EOpStep.Push:
                {
                    RaycastHit HitInfo;
                    if (Physics.Raycast(UChessboard.Instance.GetViewCamera().ScreenPointToRay(Input.mousePosition), out HitInfo))
                    {
                        selectedChess.gameObject.transform.position = HitInfo.point;


                        if (Input.GetMouseButtonDown(0))
                        {
                            Point NewPoint = UChessboard.Instance.WorldToPos(HitInfo.point);
                            if(selectedChess.GetAvailablePoints().Contains(UChessboard.Instance.ToChessPoint(NewPoint,selectedChess.campType)))
                            {
                                UChessboard.Instance.MoveChess(selectedChess, NewPoint);
                            }
                            else
                            {
                                selectedChess.ResetPosition();
                            }

                            ClearMoveTargets();

                            MyOperateStep = EOpStep.Select;
                        }
                    }
                }
                break;
           
            default:
                break;
        }

    }

    public override void TunStart()
    {
        MyOperateStep = EOpStep.Select;
    }
    public override void TunEnd()
    {
        MyOperateStep = EOpStep.None;
    }

    List<GameObject> Marks = new List<GameObject>();
    void ShowMoveTargets()
    {
        List<Point> Targets = selectedChess.GetAvailablePoints();
        for(int i=0;i<Targets.Count;i++)
        {
            Marks.Add( GameObject.Instantiate(UChessboard.Instance.MarkPrefab, 
                UChessboard.Instance.PosToWorld(UChessboard.Instance.ToChessboardPoint(Targets[i],selectedChess.campType)), 
                UChessboard.Instance.MarkPrefab.transform.rotation) as GameObject);
        }
    }

    void ClearMoveTargets()
    {
        for(int i=0;i<Marks.Count;i++)
        {
            GameObject.Destroy(Marks[i]);
        }
        Marks.Clear();
    }

    public override void DebugDraw()
    {
        GUILayout.Label(string.Format("MyOperateStep {0}", MyOperateStep));
    }
}
