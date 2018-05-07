using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour {

	//Keeps track of which team the tank is on
	public int m_PlayerNumber; 
	// Counts down time before bubble dissipates
	private float m_Countdown;

	// Use this for initialization
	void Start () {
		
	}

	void OnEnable(){
		m_Countdown = 4f;
		gameObject.transform.localScale = Vector3.one * 50f;
		Color m_Color = gameObject.GetComponentInChildren<MeshRenderer> ().material.color;
		m_Color.a = 0.55f;
		gameObject.GetComponentInChildren<MeshRenderer> ().material.color = m_Color;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_Countdown < 0f) {
			gameObject.SetActive (false);
		} else {
			m_Countdown -= Time.deltaTime;
		}
		gameObject.transform.localScale += Time.deltaTime * Vector3.one * 35f;
		gameObject.GetComponentInChildren<MeshRenderer> ().material.color -= new Color(0,0,0,0.2f * Time.deltaTime);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 9) {
			if (other.gameObject.GetComponent<TankMovement> ().m_PlayerNumber % 2 != m_PlayerNumber % 2) {
				other.gameObject.GetComponent<TankMovement> ().SpeedDown ();
			}
		}
	}
}
