using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimRotation : MonoBehaviour
{
    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_TurnSpeed = 45f;            // How fast the tank turns in degrees per second.
    private string m_AimAxisName;              // The name of the input axis for turning.
    private float m_AimInputValue;             // The current value of the turn input.
    private string m_Controller;                  //Reference to control settings
    private SerialController m_SerialController;  //Reference to the serialcontrollers
    private int m_PlayerTeamID;                 // Stores whether the player is 0 or 1 in the team 

    // Use this for initialization
    void Start()
    {
        m_PlayerNumber = gameObject.GetComponentInParent<TankMovement>().m_PlayerNumber;
        // The axes names are based on player number.
        m_AimAxisName = "Aim" + m_PlayerNumber;
        if (m_PlayerNumber % 2 != 0)
        {
            m_Controller = GameObject.Find("GameManager").GetComponent<GameManager>().BlueControl;
            if (m_Controller != "Keyboard")
            {
                m_SerialController = GameObject.Find("SerialController1").GetComponent<SerialController>();
            }
        }
        else
        {
            m_Controller = GameObject.Find("GameManager").GetComponent<GameManager>().RedControl;
            if (m_Controller != "Keyboard")
            {
                m_SerialController = GameObject.Find("SerialController2").GetComponent<SerialController>();
            }
        }
        //Set player team id
        if (m_PlayerNumber < 3)
        {
            m_PlayerTeamID = 0;
        }
        else
        {
            m_PlayerTeamID = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Controller == "Keyboard")
        {
            // Store the value of both input axes.
            m_AimInputValue = Input.GetAxis(m_AimAxisName);
        }
        else
        {
            // Store the value of both input axes.
            m_AimInputValue = m_SerialController.m_AimValues[m_PlayerTeamID];
        }
    }
    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        Turn();
    }


    private void Turn()
    {
        // Determine the number of degrees to be turned based on the input, speed and time between frames.
        float turn = m_AimInputValue * m_TurnSpeed * Time.deltaTime;

        // Make this into a rotation in the y axis.
        Vector3 turnRotation = new Vector3(0f, turn, 0f);

        // Apply this rotation to the cannon object
        transform.Rotate(turnRotation);
    }

    public void Reset()
    {
       transform.localRotation = Quaternion.identity;
    }
}
