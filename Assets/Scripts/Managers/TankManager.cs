using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TankManager
{
	// This class is to manage various settings on a tank.
	// It works with the GameManager class to control how the tanks behave
	// and whether or not players have control of their tank in the
	// different phases of the game.

	public Color m_PlayerColor;
	// This is the color this tank will be tinted.
	public Transform m_SpawnPoint;
	// The position and direction the tank will have when it spawns.
	[HideInInspector] public int m_PlayerNumber;
	// This specifies which player this the manager for.
	[HideInInspector] public GameObject m_Instance;
	// A reference to the instance of the tank when it is created.
	[HideInInspector] public Color m_CurrentColor = Color.green;
	// Used to store the current color of the tank.
	private TankMovement m_Movement;
	// Reference to tank's movement script, used to disable and enable control.
	private TankShooting m_Shooting;
	// Reference to tank's shooting script, used to disable and enable control.
	private TankHealth m_Health;
	// Reference to tank's health script, used to pass it the spawnpoint.
	private GameObject m_CanvasGameObject;
	// Used to disable the world space UI during the Starting and Ending phases of each round.


	public void Setup ()
	{
		
		// Get references to the components. Also passes the spawnpoint to the TankHealth script.
		m_Movement = m_Instance.GetComponent<TankMovement> ();
		m_Shooting = m_Instance.GetComponent<TankShooting> ();
		m_Health = m_Instance.GetComponent<TankHealth> ();
		m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas> ().gameObject;
		m_Health.m_SpawnPoint = m_SpawnPoint;

		// Set the player numbers to be consistent across the scripts.
		m_Movement.m_PlayerNumber = m_PlayerNumber;
		m_Shooting.m_PlayerNumber = m_PlayerNumber;
		m_Health.m_PlayerNumber = m_PlayerNumber;

		// Recolor Tanks
		Recolor (m_PlayerColor);
        

	}


	// Used during the phases of the game where the player shouldn't be able to control their tank.
	public void DisableControl ()
	{
		m_Movement.enabled = false;
		m_Shooting.enabled = false;

		m_CanvasGameObject.SetActive (false);
	}


	// Used during the phases of the game where the player should be able to control their tank.
	public void EnableControl ()
	{
		m_Movement.enabled = true;
		m_Shooting.enabled = true;

		m_CanvasGameObject.SetActive (true);
	}


	// Used at the start of each round to put the tank into it's default state.
	public void Reset ()
	{
		m_Instance.transform.position = m_SpawnPoint.position;
		m_Instance.transform.rotation = m_SpawnPoint.rotation;

		m_Instance.SetActive (false);
		m_Instance.SetActive (true);
	}

	// Used to change the color of tanks and spawnpoints
	public void Recolor (Color newColor)
	{
		// Get all of the renderers of the tank.
		MeshRenderer[] tank_renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();
		// Get all of the renderers of the spawn point.
		MeshRenderer[] spawnpoint_renderers = m_SpawnPoint.GetComponentsInChildren<MeshRenderer> ();

		// Go through all the renderers...
		for (int i = 0; i < tank_renderers.Length; i++) {
			//if (tank_renderers [i].material.color == m_CurrentColor) {
				// ... set their material color to the color specific to this tank.
				tank_renderers [i].material.color = newColor;
			//}
		}
		m_CurrentColor = newColor;

		// Go through all the renderers...
		for (int i = 0; i < spawnpoint_renderers.Length; i++) {
			// ... set their material color to the color specific to this spawnpoint.
			spawnpoint_renderers [i].material.color = newColor;
		}

	}
		
}
