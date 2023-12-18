using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxField : Field
{
    void Start() {} // overrides field.cs's Start method, so that no Error is thrown in case of nexField being empty
}
