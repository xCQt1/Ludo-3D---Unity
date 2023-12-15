using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndField : Field
{
    public Player Player;

    void Start() {} // overrides field.cs's Start method, so that no Error is thrown in case of nexField being empty
}
