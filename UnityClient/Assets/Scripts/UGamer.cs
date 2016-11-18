using UnityEngine;
using System.Collections;

/// <summary>
/// 下棋手基类，UBot，UPlayer继承此类
/// </summary>
public class UGamer {

    //名称
    public string name;

    //阵营
    public ECampType Camp;

    //控制器
    public UController Controller;
}
