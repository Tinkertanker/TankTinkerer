using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
	public CameraControl m_CameraControl;
	// Reference to the CameraControl script for control during different phases.
	public GameObject m_TankAPrefab;
	// Reference to the TankA prefab
	public GameObject m_TankBPrefab;
	// Reference to the TankA prefab
	public GameObject m_TankCPrefab;
	// Reference to the TankA prefab
	public TankManager[] m_Tanks;
	// A collection of managers for enabling and disabling different aspects of the tanks.
	public Setup m_Setup;
	// Reference to the Setup dialog box
	public GameObject m_Speed;
	// Reference to the speed powerup
	public GameObject m_Health;
	// Reference to the health powerup
	public GameObject m_Invulnerablity;
	// Reference to the invulnerability powerup
	public GameObject m_BlueSerialController;
	// Reference to the serialcontroller for the blue team
	public GameObject m_RedSerialController;
	// Reference to the serialcontroller for the red team
	public AudioClip m_GameMusic;
	// Stores the game music
	public float m_GameTime;
	// Used to store the time of the round
	public float m_CustomGameTime;
	// Used to store custom round time
	[HideInInspector]
	public int m_RedTeamScore;
	// Stores the score of the Blue Team
	[HideInInspector]
	public int m_BlueTeamScore;
	// Stores the score of the Red Team
	[HideInInspector]
	public string BlueControl;
	//Control Scheme + COM Port
	[HideInInspector]
	public string RedControl;
	//Control Scheme + COM Port
	[HideInInspector]
	public float m_PowerUpSpawnTime;
	// Used to store the time when the next power up should be spawned.
	public float SuddenDeathTime = 30f;
	// For changing sudden death start time (seconds)
	
	private string m_BlueTeamName;
	// Used to store the team name
	private string m_RedTeamName;
	// Used to store the team name
	private string m_BlueTeamColor = "#2A64B2";
	// Used to store the team color
	private string m_RedTeamColor = "#E52E28";
	// Used to store the team color
	private float m_RemainingTime;
	// Used to store the time elasped in the round
	private GameObject[] m_TankTypes;
	// Used to store the different types of tanks
	private List<int> m_TankChoices;
	// Used to store the players' choice of tank
	private WaitForSeconds m_ShortWait;
	// Used to have a delay whilst the round starts.
	private WaitForSeconds m_LongWait;
	// Used to have a delay whilst the round or game ends.
	private SpawnPointController[] m_SpawnPoints;
	// Reference to the spawn point script
	private GameObject[] m_PowerUps;
	// Used to store all powerups

	private bool isPaused;
	// If game is paused then PauseGame() will instead unpause
	private bool canBePaused;
	// Game can only be paused while a round is running.
	private int missingTanks;
	// The tank array defaults to 4. If we only have 2 players this will be 2 and we will subtract this from m_Tanks.Length checks.
	public Transform[] cameraTargetTemp;
	// When all tanks are destroyed at the end of the game we set the camera's targets to this transform until more tanks are spawned.
	public GameObject pauseScreen;
	// Reference to the Pause UI (panel).
	private bool suddenDeathActive;
	// Used to stop respawning if SD is active.

	public GroupStagePreset gsPreset;
	// Class to hold preset game length and sudden death timings for group stage
	public SingleEliminationPreset sePreset;
	// same as group stage preset but for single elimination
	private bool roundRunning;
	// Set true in RoundStarting and false in RoundEnding

	private string bluePort, redPort;
	// Storing each team's port numbers so can properly restart serial controllers if they're changed.
	

	


	private void Start ()
	{
		// Create the delays so they only have to be made once.
		m_ShortWait = new WaitForSeconds (2f);
		m_LongWait = new WaitForSeconds (3f);
        
		// Set Spawnpoints
		m_SpawnPoints = new SpawnPointController[] {
			GameObject.Find ("SpawnPoint1").GetComponent<SpawnPointController> (),
			GameObject.Find ("SpawnPoint2").GetComponent<SpawnPointController> ()
		};
        
		// Set Array of Powerups
		m_PowerUps = new GameObject[] { m_Health, m_Speed, m_Invulnerablity };
        
		// Set Array of Tank Types
		m_TankTypes = new GameObject[] { m_TankAPrefab, m_TankBPrefab, m_TankCPrefab };
        
		// Make extra settings inactive
		m_Setup.m_ExtraSettings.SetActive (false);
        
		// Initialize Tank Choices
		m_TankChoices = new List<int> { };

	}

	private void Update ()
	{
		// Pausing gaming
		if(Input.GetKeyDown(KeyCode.Escape) && canBePaused)
		{
			PauseGame();
		}
		// Count down
		if(roundRunning)
		{
			m_RemainingTime -= Time.deltaTime;
		}
        
		// If time not up
		if (m_RemainingTime > 0f) {
			m_Setup.m_CounterText.text = Mathf.Floor (m_RemainingTime / 60) + ":" + Mathf.Round (m_RemainingTime % 60).ToString ("00");
		}
        
		// If time to spawn powerup
		if (m_RemainingTime < m_PowerUpSpawnTime) {
			SpawnPowerUp ();
			m_PowerUpSpawnTime = m_RemainingTime - Random.Range (15f, 25f);
		}


		if(m_Setup.isActiveAndEnabled)
		{
			int presetVal = m_Setup.m_PresetSelection.value;
			if(presetVal == 0) 
			{ 
				m_Setup.m_GameLengthText.text = "Game Length: " + Mathf.Floor (gsPreset.gameLength / 60) + ":" + Mathf.Round (gsPreset.gameLength % 60).ToString ("00");
			}
			else if(presetVal == 1)
			{
				m_Setup.m_GameLengthText.text = "Game Length: " + Mathf.Floor (sePreset.gameLength / 60) + ":" + Mathf.Round (sePreset.gameLength % 60).ToString ("00");
			}
			else if(m_Setup.m_PresetSelection.value == 2)
			{
			float.TryParse(m_Setup.m_GameLength.text, out m_CustomGameTime);
			m_Setup.m_GameLengthText.text = "Game Length: " + Mathf.Floor (m_CustomGameTime / 60) + ":" + Mathf.Round (m_CustomGameTime % 60).ToString ("00");
			}
		}
	}

	private void SpawnAllTanks ()
	{
		// For all the tanks...
		for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
			// ... create them, set their player number and references needed for control.
			m_Tanks [i].m_Instance =
                Instantiate (m_TankTypes [m_TankChoices [i]], m_Tanks [i].m_SpawnPoint.position, m_Tanks [i].m_SpawnPoint.rotation) as GameObject;
			m_Tanks [i].m_PlayerNumber = i + 1;
			m_Tanks [i].Setup ();

			//Pass infomation to SpawnPointController
			if (m_Tanks [i].m_PlayerNumber % 2 == 0) {
				m_SpawnPoints [1].m_SelfTanks.Add (m_Tanks [i].m_Instance);
				m_SpawnPoints [0].m_OtherTanks.Add (m_Tanks [i].m_Instance);
			} else {
				m_SpawnPoints [0].m_SelfTanks.Add (m_Tanks [i].m_Instance);
				m_SpawnPoints [1].m_OtherTanks.Add (m_Tanks [i].m_Instance);
			}
		}
		m_SpawnPoints [0].m_TanksSpawned = true;
		m_SpawnPoints [1].m_TanksSpawned = true;
	}


	private void SetCameraTargets ()
	{
		// Create a collection of transforms the same size as the number of tanks.
		Transform[] targets = new Transform[m_Tanks.Length - missingTanks];

		// For each of these transforms...
		for (int i = 0; i < targets.Length; i++) {
			// ... set it to the appropriate tank transform.
			targets [i] = m_Tanks [i].m_Instance.transform;
		}

		// These are the targets the camera should follow.
		m_CameraControl.m_Targets = targets;
	}


	// This is called from start and will run each phase of the game one after another.
	private IEnumerator GameLoop ()
	{
		// Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
		yield return StartCoroutine (RoundStarting ());

		// Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
		yield return StartCoroutine (RoundPlaying ());

		//IsGameTied();

		if(IsGameTied() && !suddenDeathActive)
		{
			yield return StartCoroutine(SuddenDeath(SuddenDeathTime));
		}
		Debug.Log("game not tied");
		RoundEnding ();
		//EndGame();
		
	}


	private IEnumerator SuddenDeath(float suddenDeathExtraTime)
	{
		m_Setup.m_SuddenDeathText.text = "<color=#ff0000ff>Sudden Death! \nNo more respawns</color>";
		suddenDeathActive = true;
		canBePaused = true;
		m_RemainingTime = suddenDeathExtraTime;
		while(m_RemainingTime > 0f)
		{
			yield return null;
		}
	}
	
	private bool IsGameTied()
	{
		if(m_BlueTeamScore == m_RedTeamScore)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private void RemoveTanksFromSpawnpoints()
	{
		for(int i = 0; i < m_Tanks.Length - missingTanks; i++)
		{
			m_SpawnPoints[0].m_SelfTanks.Clear();
			m_SpawnPoints[1].m_SelfTanks.Clear();
			m_SpawnPoints[0].m_OtherTanks.Clear();
			m_SpawnPoints[1].m_OtherTanks.Clear();
		}
	}

	private void KillAllTanks()
	{
		for(int i = 0; i < m_Tanks.Length - missingTanks; i++)
		{
			Destroy(m_Tanks[i].m_Instance);
		}
	}


	private IEnumerator RoundStarting ()
	{
		roundRunning = true;
		// As soon as the round starts reset the tanks and make sure they can't move. Also reset the counter
		ResetAllTanks ();
		DisableTankControl ();
		suddenDeathActive = m_Setup.m_SuddenDeathToggle.isOn;

		int presetVal = m_Setup.m_PresetSelection.value;

		if(presetVal == 0)
		{
			m_RemainingTime = gsPreset.gameLength;
			SuddenDeathTime = gsPreset.suddenDeathExtraTime;
		}
		else if(presetVal == 1)
		{
			m_RemainingTime = sePreset.gameLength;
			SuddenDeathTime = sePreset.suddenDeathExtraTime;
		}
		else if(presetVal == 2)
		{
			m_RemainingTime = m_CustomGameTime;
			SuddenDeathTime = 30f;
		}
		

		//Initializes the score to zero
		m_RedTeamScore = 0;
		m_BlueTeamScore = 0;
		UpdateScore ();

		//Start PowerUp counter
		m_PowerUpSpawnTime = m_RemainingTime - Random.Range (5f, 20f);

		// Snap the camera's zoom and position to something appropriate for the reset tanks.
		m_CameraControl.SetStartPositionAndSize ();

		// Display text showing the players that the game has started.
		m_Setup.m_MessageText.text = "Game Start!";

		// Wait for the specified length of time until yielding control back to the game loop.
		yield return m_ShortWait;
	}


	private IEnumerator RoundPlaying ()
	{
		// As soon as the round begins playing let the players control the tanks.
		EnableTankControl ();
		if(suddenDeathActive)
		{
			m_Setup.m_SuddenDeathText.text = "<color=#ff0000ff>Sudden Death! \nNo more respawns</color>";
		}
		
		canBePaused = true;

		// Clear the text from the screen.
		m_Setup.m_MessageText.text = string.Empty;

		// While there is time remaining
		while (m_RemainingTime > 0f) {
			// ... return on the next frame.
			yield return null;
		}
	}


	private void RoundEnding ()
	{
		roundRunning = false;
		// Stop tanks from moving.
		DisableTankControl ();
		m_Setup.m_EndGameButton.SetActive(true);
		canBePaused = false;
		StopAllCoroutines();

		m_Setup.m_SuddenDeathText.text = "";

		// Get a message based on the scores and whether or not there is a game winner and display it.
		if (m_RedTeamScore > m_BlueTeamScore) {
			m_Setup.m_MessageText.text = "<color=" + m_RedTeamColor + "> " + m_RedTeamName + " wins!" + "</color>";
		} else if (m_RedTeamScore < m_BlueTeamScore) {
			m_Setup.m_MessageText.text = "<color=" + m_BlueTeamColor +"> " + m_BlueTeamName + " wins!" + "</color>";
		} else {
			// By default when a round ends there are no winners so the default end message is a draw.
			m_Setup.m_MessageText.text = "DRAW!";
		}
   
		// Turn off spawn points
		for (int i = 0; i < m_SpawnPoints.Length; i++) {
			m_SpawnPoints [i].m_TanksSpawned = false;
		}

		// Wait for the specified length of time until yielding control back to the game loop.
		//yield return m_LongWait;
	}

	private IEnumerator StartGame ()
	{
		//Close setup dialog
		m_Setup.gameObject.SetActive (false);

		//Show game objective
		m_Setup.m_MessageText.text = "Capture The Flag!";

		// Wait for the specified length of time 
		yield return m_LongWait;

		//Initialize game
		SpawnAllTanks ();
		SetCameraTargets ();

		//Remove game title
		m_Setup.m_MessageText.text = string.Empty;

		//Start game loop
		StartCoroutine (GameLoop ());

	}

	// This function is used to turn all the tanks back on and reset their positions and properties.
	private void ResetAllTanks ()
	{
		for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
			m_Tanks [i].Reset ();
		}
	}


	private void EnableTankControl ()
	{
		for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
			m_Tanks [i].EnableControl ();
		}
	}


	private void DisableTankControl ()
	{
		for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
			m_Tanks [i].DisableControl ();
		}
	}

	public void OnClickButton (string choice)
	{
		if (choice == "start") {
			// Set team names
			m_BlueTeamName = (m_Setup.m_BlueTeamNameInput.text == "") ? "Blue Team" : m_Setup.m_BlueTeamNameInput.text;
			m_RedTeamName = (m_Setup.m_RedTeamNameInput.text == "") ? "Red Team" : m_Setup.m_RedTeamNameInput.text;

			// Set player control scheme/COM port
			BlueControl = m_Setup.m_BlueControl.options [m_Setup.m_BlueControl.value].text;
			RedControl = m_Setup.m_RedControl.options [m_Setup.m_RedControl.value].text;

			// Pass control infomation to SpawnPoints
			m_SpawnPoints [0].m_Controller = BlueControl;
			m_SpawnPoints [1].m_Controller = RedControl;

			// Set serialcontroller COM port
			if (BlueControl != "Keyboard" && GetPortName(m_Setup.m_BluePort.text) != bluePort) {
				if(m_BlueSerialController.activeInHierarchy)
				{
					m_BlueSerialController.SetActive(false);
				}
				m_BlueSerialController.GetComponent<SerialController> ().portName = GetPortName (m_Setup.m_BluePort.text);
				m_BlueSerialController.SetActive (true);
				bluePort = GetPortName(m_Setup.m_BluePort.text);
			}
			if (RedControl != "Keyboard" && GetPortName(m_Setup.m_RedPort.text) != redPort) {
				if(m_RedSerialController.activeInHierarchy)
				{
					m_RedSerialController.SetActive(false);
				}
				m_RedSerialController.GetComponent<SerialController> ().portName = GetPortName (m_Setup.m_RedPort.text);
				m_RedSerialController.SetActive (true);
				redPort = GetPortName(m_Setup.m_RedPort.text);
			}


			// Store choice of tank
			m_TankChoices.Add (m_Setup.m_P1Dropdown.value);
			m_TankChoices.Add (m_Setup.m_P2Dropdown.value);

			// Alternative settings for 1v1/2v2
			if (m_Setup.m_Gamemode.value == 0) {
				missingTanks = 2;
			} else {
				missingTanks = 0;
				m_TankChoices.Add (m_Setup.m_P3Dropdown.value);
				m_TankChoices.Add (m_Setup.m_P4Dropdown.value);
			}

			// Start music
			gameObject.GetComponent<AudioSource> ().clip = m_GameMusic;
			gameObject.GetComponent<AudioSource> ().Play ();

			//Perform initialization
			StartCoroutine (StartGame ());

		}
	}

	public void UpdateScore (string team = "")
	{
		
		m_Setup.m_BlueScoreText.text = "" + m_BlueTeamScore;
		m_Setup.m_RedScoreText.text = "" + m_RedTeamScore;

		if (team == "Red") {
			StartCoroutine (DisplayText ("<color="+ m_RedTeamColor +"> " + m_RedTeamName + " has captured a flag!" + "</color>"));
		} else if (team == "Blue") {
			StartCoroutine (DisplayText ("<color="+ m_BlueTeamColor +"> " + m_BlueTeamName + " has captured a flag!" + "</color>"));
		} else {
			// Enable GUI
			m_Setup.m_Image.SetActive (true);
			m_Setup.m_BlueTeamText.text = m_BlueTeamName;
			m_Setup.m_RedTeamText.text = m_RedTeamName;
		}
		if(suddenDeathActive && team == "Red" | team == "Blue")
		{
			RoundEnding();
		}
	}

	private void SpawnPowerUp ()
	{
		Vector3 position = new Vector3 (Random.Range (-40.0f, 40.0f), 2.1f, Random.Range (-40.0f, 40.0f));
		Instantiate (m_PowerUps [Random.Range (0, 3)], position, Quaternion.identity);
	}

	public void ToggleExtraSettings ()
	{
		// If 2v2 has not been selected
		if (m_Setup.m_Gamemode.value == 0) {
			// Disable extra settings
			m_Setup.m_ExtraSettings.SetActive (false);
		} else {
			// Enable Extra settings
			m_Setup.m_ExtraSettings.SetActive (true);
		}
	}

	public void EnableCustomGameLength()
	{
		m_Setup.m_GameLength.interactable = !m_Setup.m_GameLength.interactable;
	}

	// Shows the inputfield for entering COM number
	public void ToggleSerial ()
	{
		if (m_Setup.m_BlueControl.value == 1) {
			m_Setup.m_BluePort.gameObject.SetActive (true);
		} else {
			m_Setup.m_BluePort.gameObject.SetActive (false);
		}
		if (m_Setup.m_RedControl.value == 1) {
			m_Setup.m_RedPort.gameObject.SetActive (true);
		} else {
			m_Setup.m_RedPort.gameObject.SetActive (false);
		}
	}

	// Starts the coroutine to respawn the tank
	public void Respawn (GameObject tank)
	{
        if (!suddenDeathActive) // only starts the coroutine avalanche if suddendeath is not active.
        {
            StartCoroutine(RespawnTank(tank));
        }
	}

	private IEnumerator RespawnTank (GameObject tank)
	{
		yield return m_ShortWait;
		tank.SetActive (true);
	}

	private IEnumerator DisplayText (string text)
	{
		// Show text
		m_Setup.m_MessageText.text = text;

		// Wait for the specified length of time 
		yield return m_ShortWait;

		// Remove text
		m_Setup.m_MessageText.text = string.Empty;
        
	}

	private string GetPortName (string port)
	{
		// If running on windows
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {

			if (int.Parse (port) > 9) {
				return "\\\\.\\COM" + port;
			} else {
				return "COM" + port;

			}
		} else {
			return "/dev/cu.usbmodem" + port;

		}
	}

	public void RecolorTanks (string team, string htmlColor)
	{
		Color newColor;
		ColorUtility.TryParseHtmlString (htmlColor, out newColor); 
		if (LABColor.Compare (new LABColor (newColor), new LABColor (new Color (255, 187, 77))) > 20f) {
			if (team == "Blue" && LABColor.Compare (new LABColor (newColor), new LABColor (m_Tanks[1].m_CurrentColor)) > 40f) {
				for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
					if (i % 2 == 0) {
						m_Tanks [i].Recolor (newColor);
					}
				}
				m_BlueTeamColor = htmlColor;
				m_Setup.m_BlueTeamText.color = newColor;
				m_Setup.m_BlueScoreText.color = newColor;
			} else if (team == "Red" && LABColor.Compare (new LABColor (newColor), new LABColor (m_Tanks[0].m_CurrentColor)) > 40f){
				for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
					if (i % 2 != 0) {
						m_Tanks [i].Recolor (newColor);
					}
				}
				m_RedTeamColor = htmlColor;
				m_Setup.m_RedTeamText.color = newColor;
				m_Setup.m_RedScoreText.color = newColor;
			}
		}
	}

	// Starts the coroutine to execute ultimate
	public void AUlti (int playernumber)
	{
		StartCoroutine (GlobalKill(playernumber));
	}

	private IEnumerator GlobalKill (int playernumber)
	{
		yield return m_LongWait;
		for (int i = 0; i < m_Tanks.Length - missingTanks; i++) {
			if (i % 2 == playernumber % 2 && m_Tanks [i].m_Instance.GetComponent<TankHealth> ().m_CurrentHealth < 40f) {
				m_Tanks [i].m_Instance.GetComponent<TankHealth> ().TakeDamage (40f);
			}
		}

	}


	// Pausing game by setting timescale to 0. Better ways to do this but this is easy and was in a hurry.
	public void PauseGame()
	{
		isPaused = !isPaused;
		if(Time.timeScale == 1)
		{
			pauseScreen.SetActive(true);
			Time.timeScale = 0;
		}
		else
		{
			pauseScreen.SetActive(false);
			Time.timeScale = 1;
		}
	}
	
	// Ending game manually
	public void EndGame()
	{
		m_TankChoices.Clear();
		canBePaused = false;
		m_Setup.m_EndGameButton.SetActive(false);
		m_Setup.m_Image.SetActive (false);
		StopAllCoroutines();
		DisableTankControl();
		suddenDeathActive = false;
		for (int i = 0; i < m_SpawnPoints.Length; i++) 
		{
			m_SpawnPoints [i].m_TanksSpawned = false;
		}		
		m_CameraControl.m_Targets = cameraTargetTemp;
		RemoveTanksFromSpawnpoints();
		KillAllTanks();
		m_Setup.gameObject.SetActive (true);
		m_RemainingTime = 0f;
		m_Setup.m_CounterText.text = "";
		m_Setup.m_BlueScoreText.text = "";
		m_Setup.m_RedScoreText.text = "";
		m_Setup.m_BlueTeamText.text = "";
		m_Setup.m_RedTeamText.text = "";
		m_Setup.m_SuddenDeathText.text = "";
		// m_BlueSerialController.SetActive(false);
		// m_RedSerialController.SetActive(false);
	}

	public void OnGamePresetChanged()
	{
		int presetVal = m_Setup.m_PresetSelection.value;
		if(presetVal == 2)
		{
			m_Setup.m_GameLength.interactable = true;
			m_Setup.m_SuddenDeathToggle.interactable = true;
		}
		else
		{
			m_Setup.m_GameLength.interactable = false;
			m_Setup.m_SuddenDeathToggle.interactable = false;
			m_Setup.m_SuddenDeathToggle.isOn = false;

		}
		
	}


	[System.Serializable]
	public class GroupStagePreset
	{
		public float gameLength = 240f;
		public float suddenDeathExtraTime = 30f;

	}

	[System.Serializable]
	public class SingleEliminationPreset
	{
		public float gameLength = 360f;
		public float suddenDeathExtraTime = 120f;
	}
}