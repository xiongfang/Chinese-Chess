using UnityEngine;
using System.Collections;

/// <summary>
/// 控制器基类
/// </summary>
public class UController  {
    //我的阵营
    public ECampType MyCamp;
    public UGamer Gamer;

    public virtual void Update() { }

    public virtual void DebugDraw() { }

    //轮开始通知
    public virtual void TunStart() { }
    public virtual void TunEnd() { }

    public virtual  void OnAttach(UGamer Gamer)
    {
        MyCamp = Gamer.Camp;
        this.Gamer = Gamer;
    }
    public virtual void OnDetach(UGamer Gamer) { }
}
