using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setup : MonoBehaviour
{
    public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
    public Text m_CounterText;                  // References to the overlay Text displaying the counter
    public Text m_RedScoreText;                 // References to the overlay Text displaying the red team's score
    public Text m_BlueScoreText;                // References to the overlay Text displaying the blue team's score
    public Text m_BlueTeamText;
    public Text m_RedTeamText;
    public Dropdown m_Gamemode;                 // Reference to the Gamemode dropdown
    public Dropdown m_BlueControl;             // Reference to the dropdown for blue team
    public Dropdown m_RedControl;              // Reference to the dropdown for red team
    public InputField m_BluePort;              //Reference to the port for the blue controller
    public InputField m_RedPort;               //Reference to the port for the red controller
    public Dropdown m_P1Dropdown;              // Reference to the dropdown for player 1
    public Dropdown m_P2Dropdown;              // Reference to the dropdown for player 2
    public Dropdown m_P3Dropdown;              // Reference to the dropdown for player 3
    public Dropdown m_P4Dropdown;              // Reference to the dropdown for player 4
    public InputField m_BlueTeamNameInput;        // References the blue team inputfield
    public InputField m_RedTeamNameInput;        // References the red team inputfield
    public GameObject m_Image;                  // References the HUD overlay
    public GameObject m_ExtraSettings;          // Reference to the extra settings
    public Text m_SuddenDeathText;              // Reference to Sudden Death display text
    public InputField m_GameLength;             // Reference to custom game length field
    public Toggle m_CustomLengthToggle;         // Reference to custom game length toggle
    public Text m_GameLengthText;               // Reference to text to display length the game will be

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
