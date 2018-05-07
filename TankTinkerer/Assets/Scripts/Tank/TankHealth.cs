using UnityEngine;
using UnityEngine.UI;


public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_FillImage;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.
    public GameObject m_HealingPrefab;                  // A prefab that will be instantiated in Awake, then used whenever the tank is healing.
    public GameObject m_SparklePrefab;                  // A prefab tht will be instantiated in Awake, then used when the tank is invulnerable
    public Transform m_SpawnPoint;                      // Reference to the SpawnPoint, used to reset the tank. 
    public bool m_Healing;                              // Stores whether the tank is regenerating health
    public float m_HealAmount;                          // Stores the amount of health the tank regenerates per tick when on the spawnpoint.
    public bool m_HasFlag;                              // Stores whether the player is in possesion of the flag
	public int m_PlayerNumber;							// Store the player number
	public float m_CurrentHealth;                      // How much health the tank currently has.

	private string m_Controller;                        // Reference to control settings
    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system that will play when the tank is destroyed.
    private ParticleSystem m_SparkleParticles;          // The particle system that will play when the tank is invulnerable
    private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?
    private ParticleSystem m_HealingParticles;          // The particle system that will play when the tank is healing.
    private float m_Countdown;                          // Stores the time left of invulnerability
    private bool m_IsInvulnerable;                      // Stores whether the player is invulnerable
	private SerialController m_SerialController;        // Stores reference to serial controller
	private int m_PlayerTeamID;							// Stores the player ID within a team

    private void Awake()
    {
        // Instantiate the explosion prefab and get a reference to the particle system on it.
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();

        // Get a reference to the audio source on the instantiated prefab.
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        // Disable the prefab so it can be activated when it's required.
        m_ExplosionParticles.gameObject.SetActive(false);

        // Instantiate the healing prefab and get a reference to the particle system on it.
        m_HealingParticles = Instantiate(m_HealingPrefab).GetComponent<ParticleSystem>();

        // Disable the prefab so it can be activated when it's required.
        m_HealingParticles.gameObject.SetActive(false);

        // Instantiate the sparkle prefab and get a reference to the particle system on it.
        m_SparkleParticles = Instantiate(m_SparklePrefab).GetComponent<ParticleSystem>();

        // Disable the prefab so it can be activated when it's required.
        m_SparkleParticles.gameObject.SetActive(false);

        //Set the heal amount
        m_HealAmount = 0.4f;

        //Set the flag marker
        m_HasFlag = false;

        //Initialize the countdown timer and respawn timer
        m_Countdown = 0f;
    }

	private void Start(){
	
		//Initialize Control Scheme
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

    private void Update()
    {
        if (m_Healing && m_CurrentHealth + m_HealAmount <= m_StartingHealth)
        {
            // Increase tank health and update health bar
            m_CurrentHealth += m_HealAmount;
            SetHealthUI();
            m_HealingParticles.transform.position = transform.position;
        }
        else if (m_Healing && m_CurrentHealth <= m_StartingHealth)
        {
            // Increase tank health and update health bar
            m_CurrentHealth = m_StartingHealth;
            SetHealthUI();
            StopHeal();
        }
        if (m_Countdown > 0f)
        {
            m_Countdown -= Time.deltaTime;
            m_SparkleParticles.transform.position = transform.position;
        }
        else if (m_Countdown < 0f)
        {
            m_IsInvulnerable = false;
            m_Countdown = 0f;
            // Stop the particle system of the tank healing
            m_SparkleParticles.Stop();
            m_SparkleParticles.gameObject.SetActive(false);
            if (m_Countdown == 0)
            {
                m_Countdown = -1f;
            }
        }
    }

    private void OnEnable()
    {
        // When the tank is enabled, reset the tank's health and whether or not it's dead.
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        // Update the health slider's value and color.
        SetHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (!m_IsInvulnerable)
        {
            // Reduce current health by the amount of damage done.
            m_CurrentHealth -= amount;

            // Change the UI elements appropriately.
            SetHealthUI();

            // If the current health is at or below zero and it has not yet been registered, call OnDeath.
			if (m_CurrentHealth <= 0f && !m_Dead) {
				OnDeath ();
			} else {
				// Send a message to the reciever
				if (m_Controller != "Keyboard") {
					m_SerialController.SendSerialMessage (m_PlayerTeamID + "D");
				}
			}
        }

    }

    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    public void StartHeal()
    {
        m_HealingParticles.gameObject.SetActive(true);
        // Play the particle system of the tank healing
        m_HealingParticles.Play();

    }

    public void StopHeal()
    {
        // Stop the particle system of the tank healing
        m_HealingParticles.Stop();
        m_HealingParticles.gameObject.SetActive(false);
    }

    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Move the instantiated explosion prefab to the tank's position and turn it on.
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        // Play the particle system of the tank exploding.
        m_ExplosionParticles.Play();

        // Play the tank explosion sound effect.
        m_ExplosionAudio.Play();

        // Stop the healing particles from playing
        m_SparkleParticles.gameObject.SetActive(false);

        // Return the flag
        m_HasFlag = false;
       
		// Send a message to the reciever
		if (m_Controller != "Keyboard") {
			m_SerialController.SendSerialMessage (m_PlayerTeamID + "X");
		}

        // Disable and reset tank
        gameObject.transform.position = m_SpawnPoint.position;
        gameObject.transform.rotation = m_SpawnPoint.rotation;
        gameObject.GetComponentInChildren<AimRotation>().Reset();
        gameObject.SetActive(false);
        GameObject.FindObjectOfType<GameManager>().Respawn(gameObject);
    }

    public void RestoreHealth() {
        // Calculate the maximum health that can be added
        float MaxAdd = m_StartingHealth - m_CurrentHealth;
        if (MaxAdd < 20)
        {
            m_CurrentHealth = m_StartingHealth;
        }
        else if (MaxAdd < 60){
            m_CurrentHealth += Random.Range(20f, MaxAdd);
        }
        else
        {
            m_CurrentHealth += Random.Range(20f, 60f);
        }
        SetHealthUI();
    }

    public void BecomeInvulnerable() {
        m_IsInvulnerable = true;
        m_Countdown = 10f;
        m_SparkleParticles.gameObject.SetActive(true);
        // Play the sparkle particles
        m_SparkleParticles.Play();
        // Set the position of the particles to the player
        m_SparkleParticles.transform.position = transform.position;
    }
}
