using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private float m_Countdown; //Stores the time left for the powerup
    // Use this for initialization
    void Start()
    {
        m_Countdown = 30f;
    }

    // Update is called once per frame
    void Update()
    {

        transform.Rotate(0f, 150f * Time.deltaTime, 0f); //rotates 1 degrees per second around y axis
        if (m_Countdown < 0f)
        {
            Object.Destroy(gameObject);
        }
        else
        {
            m_Countdown -= Time.deltaTime;
            if (m_Countdown == 0f)
            {
                m_Countdown = -1f;
            }
        }
    }
		

    private void OnTriggerEnter(Collider other)
    {
        // if other is not player
        if (other.gameObject.layer == 11)
        { }
        else if (other.gameObject.layer != 9)
        {
            m_Countdown = -1f;
            GameObject.Find("GameManager").GetComponent<GameManager>().m_PowerUpSpawnTime += 30f;
        }
        else if (other.gameObject.layer == 9)
        {
            m_Countdown = -1f;
            if (gameObject.tag == "Health")
            {
                other.GetComponent<TankHealth>().RestoreHealth();
                //Debug.Log("Player " + other.GetComponent<TankMovement>().m_PlayerNumber + " has obtained health");
            }
            else if (gameObject.tag == "Invulnerability")
            {
                other.GetComponent<TankHealth>().BecomeInvulnerable();
                //Debug.Log("Player " + other.GetComponent<TankMovement>().m_PlayerNumber + " has obtained invulnerability");
            }
            else if (gameObject.tag == "Speed")
            {
                other.GetComponent<TankMovement>().SpeedUp();
                //Debug.Log("Player " + other.GetComponent<TankMovement>().m_PlayerNumber + " has obtained speed");
            }
        }
    }
}
