using UnityEngine;
using System.Collections;

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
    Chess selectedChess;

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
                            Chess HitChess = UChessboard.Instance[UChessboard.Instance.WorldToPos(HitInfo.point)];
                            if (HitChess != null && HitChess.campType == MyCamp)
                            {
                                selectedChess = HitChess;
                                MyOperateStep = EOpStep.Push;
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
                            UChessboard.Instance.MoveChess(selectedChess, NewPoint);

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

    public override void DebugDraw()
    {
        GUILayout.Label(string.Format("MyOperateStep {0}", MyOperateStep));
    }
}
