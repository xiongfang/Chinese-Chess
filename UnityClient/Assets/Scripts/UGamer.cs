using UnityEngine;
using System.Collections;

/// <summary>
/// 下棋手基类，UBot，UPlayer继承此类
/// </summary>
public class UGamer {

    //对弈的棋盘
    public UChessboard Chessboard;

    //名称
    public string name;

    //阵营
    public ECampType Camp;

    //控制器
    private UController _controller;

    public UController Controller
    {
        get { return _controller; }
    }

    public void Attach(UController controller)
    {
        if(Controller!=null)
        {
            Controller.OnDetach(this);
        }
        _controller = controller;
        if(Controller!=null)
        {
            Controller.OnAttach(this);
        }
    }
}
