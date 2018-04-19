using System;
using UnityEngine;
using System.Collections.Generic;


public class SpawnPointController : MonoBehaviour
{

    public List<GameObject> m_OtherTanks;   // Reference to enemy tanks
    public List<GameObject> m_SelfTanks;    // Reference to own tanks
    public GameManager m_GameManager;       // References to game manager
    public bool m_TanksSpawned;             // Stores whether the game is in progress
    public string m_Team;                   // Stores which team the spawnpoint is on
	public SerialController m_SerialController;		// Reference to the corresponding serial controller

    private Vector3 m_OtherFlagPosition;   // Store the position of the opponent's flag
    private GameObject m_OtherFlag;        // Reference to opponent's flag
	private string m_Controller;			// Reference to the control scheme

	void Start()
    {
        // Obtain reference to opponents flag
        if (m_Team == "Red")
        {
            m_OtherFlag = GameObject.FindGameObjectWithTag("BlueFlag");
			m_Controller = m_GameManager.RedControl;

        }
        else
        {
            m_OtherFlag = GameObject.FindGameObjectWithTag("RedFlag");
			m_Controller = m_GameManager.BlueControl;
        }
        // Store the original position
        m_OtherFlagPosition = m_OtherFlag.transform.position;
    }
    
    void Update()
    {
        // Only call once game begins
        if (m_TanksSpawned)
        {
            bool selfHasFlag = false;
            for (int i=0; i<m_SelfTanks.Count; i++)
            {
                if (m_SelfTanks[i].GetComponent<TankHealth>().m_HasFlag)
                {
                    // Set flag position to tank position
                    m_OtherFlag.transform.position = m_SelfTanks[i].transform.position;
                    selfHasFlag = true;
                }
            }
            if (!selfHasFlag)
            { 
                // Set flag position to original position
                m_OtherFlag.transform.position = m_OtherFlagPosition;

            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // If is an enemy tank
        if (m_OtherTanks.Contains(collider.gameObject) && !OtherHasFlag())
        {
            collider.GetComponent<TankHealth>().m_HasFlag = true;
			if (m_Controller != "Keyboard") {
				m_SerialController.SendSerialMessage ("C");
			}
        }
        // If is own tank
        if (m_SelfTanks.Contains(collider.gameObject))
        {
            // Start healing
            collider.GetComponent<TankHealth>().m_Healing = true;
            collider.GetComponent<TankHealth>().StartHeal();
            // If tank has flag
            if (collider.GetComponent<TankHealth>().m_HasFlag)
            {
                collider.GetComponent<TankHealth>().m_HasFlag = false;
                if (m_Team == "Red")
                {
                    m_GameManager.GetComponent<GameManager>().m_RedTeamScore += 1;
                    m_GameManager.GetComponent<GameManager>().UpdateScore(m_Team);
                }
                else
                {
                    m_GameManager.GetComponent<GameManager>().m_BlueTeamScore += 1;
                    m_GameManager.GetComponent<GameManager>().UpdateScore(m_Team);
                }
                m_OtherFlag.transform.position = m_OtherFlagPosition;

            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (m_SelfTanks.Contains(collider.gameObject))
        {
            // Stop healing
            collider.gameObject.GetComponent<TankHealth>().StopHeal();
            collider.gameObject.GetComponent<TankHealth>().m_Healing = false;
        }
    }

    private bool OtherHasFlag()
    {
        for (int i = 0; i < m_OtherTanks.Count; i++)
        {
            if (m_OtherTanks[i].GetComponent<TankHealth>().m_HasFlag)
            {
                return true;
            }
        }
        return false;
    }
}
