using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    private static CanvasScript instance = null;
    public Text variablesText;
    public AutonomousScript autonomousScript;
    public Text streamingLabel;
    public UnityCapture capture;

    void Start() {
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

    void Update()
    {
        capture = GameObject.Find("/Vehicle1/AutonomousCamera").GetComponent<UnityCapture>();
        variablesText.text = "Vertical: " + autonomousScript.verticalVal + "\n"
        + "Horizontal: " + autonomousScript.horizontalVal + "\n"
        + "Handbrake: " + autonomousScript.handbrake.ToString() + "\n"
        + "Pause: " + autonomousScript.pause + "\n"
        + "isForward: " + autonomousScript.isForward;
        if(streamingLabel.text == "On")
        {
            capture.sendCapture = true;
        }
        else
        {
            capture.sendCapture = false;
        }
    }
}
