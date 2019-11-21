using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading;
using System.IO;

[Serializable]
public class ControlPackage
{
    public int vertical = -1;
    public int horizontal = -1;
    public bool pause;
    public bool handbrake;
    public bool reset;
}

public class AutonomousScript : MonoBehaviour
{
    private static AutonomousScript instance = null;
    public float _verticalVal = 0;
    public float verticalVal
    {
        get
        {
            return _verticalVal;
        }
        set
        {
            if(value > 255) value = 255.0f;
            if(value < 0) value = 0.0f;
            _verticalVal = value;
        }
    }
    public float _horizontalVal = 0;
    public float horizontalVal
    {
        get
        {
            return _horizontalVal;
        }
        set
        {
            if(value > 254) value = 254.0f;
            _horizontalVal = value - 127;
        }
    }
    public bool pause = false;
    public bool handbrake = false;
    public bool reset = false;

    void Start()
    {
        if( instance == null )
        {
            instance = this;
        }
        else if( this != instance )
        {
            Destroy( gameObject );
            return;
        }
        DontDestroyOnLoad(this);
    }
}
