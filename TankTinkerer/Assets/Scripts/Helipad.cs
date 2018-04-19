using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helipad : MonoBehaviour
{
    public GameObject m_Other;           // Reference to other helipad
    [HideInInspector]
    public bool m_IsOccupied;            // Stores whether there is a tank at the teleporter 
    private List<float> m_Timers;        // Stores countdowns before tank is teleported
    private List<GameObject> m_Tanks;    // Stores tanks currently in contact
    private float m_DelayTime = 1f;      // Constant delay time before tanks are teleported
    private float m_Cooldown = 0f;       // Constant cooldown time before Helipads are reactivated


    // Use this for initialization
    void Start()
    {
        m_Timers = new List<float> { };
        m_Tanks = new List<GameObject> { };
        m_IsOccupied = false;

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < m_Timers.Count; i++)
        {
            // Increment timer
            m_Timers[i] += Time.deltaTime;
            // If delay is up and other helipad is not occupied
            if (m_Timers[i] > m_DelayTime && !m_Other.GetComponent<Helipad>().m_IsOccupied && m_Cooldown == 0f && m_Other.GetComponent<Helipad>().m_Cooldown == 0f)
            {
                // Move tank
                m_Tanks[i].transform.position = new Vector3(m_Other.transform.position.x, 0, m_Other.transform.position.z);
                GameObject.Find("GameManager").GetComponent<GameManager>().Respawn(m_Tanks[i]);
                m_Tanks[i].SetActive(false);
                m_IsOccupied = false;
                m_Timers.RemoveAt(i);
                m_Tanks.RemoveAt(i);
                m_Cooldown = 6f;
                m_Other.GetComponent<Helipad>().m_Cooldown = 6f;
                gameObject.GetComponent<AudioSource>().Play();
            }
        }
        if (m_Cooldown > 0f)
        {
            m_Cooldown -= Time.deltaTime;
        }
        else if (m_Cooldown < 0f)
        {
            m_Cooldown = 0f;
        }
    }

    // Ontriggerenter is called when an object collides with the helipad
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            m_IsOccupied = true;
            m_Tanks.Add(other.gameObject);
            m_Timers.Add(0f);
        }
    }

    // Ontriggerexit is called when an object is no longer colliding with the helipad
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            m_IsOccupied = false;
            m_Timers.RemoveAt(m_Tanks.IndexOf(other.gameObject));
            m_Tanks.RemoveAt(m_Tanks.IndexOf(other.gameObject));
        }
    }
}
