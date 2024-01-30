using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identifier : MonoBehaviour
{
    //This instance will be present in every game mode. This is to allow all prefabs to assign game mode on Awake

    public bool isTDM = false;
    public bool isChaos = false;
    public bool isSurvival = false;

}
