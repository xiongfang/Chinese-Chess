using UnityEngine;
using System.Collections;

public class Config  {

    public static int NumInputsPerNeuron = 10;
    public static int NumNeuronPerHiddenLayer = 10;
    public static int NumHiddenLayer = 1;
    public static int NumInputs = 10;
    public static int NumOutputs = 3;
    public static double Bias = 1.0;
    public static double ActivationResponse = 1.0f;

    //基因算法配置
    public static float MutationRate = 0.1f;    //突变率
    public static double MaxPerturbatlon = 0.2f; //最大突变值
}
