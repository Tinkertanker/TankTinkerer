using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;              // Used to identify the different players.
    public Rigidbody m_Projectile;                   // Prefab of the shell/bullet.
    public GameObject m_RocketProp;                 // Prefab of the rocket prop
    public Transform m_CannonTransform;         // A child of the tank where the rocketprops are spawned.
    public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
    public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
    public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
    public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
    public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
    public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
    public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
    public LayerMask m_TankMask;                        // Used to filter what the explosion affects, this should be set to "Players".


    private string m_FireButton;                // The input axis that is used for launching shells.
    private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
    private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
    private bool m_Fired = true;                // Whether or not the shell has been launched with this button press.
    private bool m_Charging = false;            // Whether the shell is currently charging.
    private SerialController m_SerialController; // Reference to the serialcontroller
    private string m_Controller;                 // Reference to the control settings in GameManager
    private int m_RocketNumber;                  // Stores how many rockets have been fired
    private float m_Cooldown;                   // Stores the cooldown on firing.
    private bool m_CanShoot;                    // Stores whether the tank can fire.
    private float m_RocketCooldown;             // Stores when a new rocket will be spawned
    private int m_PlayerTeamID;                 // Stores whether the player is 0 or 1 in the team 

    private void Awake()
    {
        //Initialize fire cooldown
        m_Cooldown = 0f;
        m_CanShoot = true;
    }

    private void OnEnable()
    {
        if (tag == "A")
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }

    }


    private void Start()
    {
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

        // The fire axis is based on the player number.
        m_FireButton = "Fire" + m_PlayerNumber;

        // The rate that the launch force charges up is the range of possible forces by the max charge time.
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

        // If tank is C, initialize rockets and start rocket cooldown
        if (tag == "C")
        {
            m_RocketNumber = 16;
            m_RocketCooldown = 1.4f;
        }

    }


    private void Update()
    {
        if (tag == "A")
        {

            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_MinLaunchForce;

            if (m_Controller == "Keyboard" && m_CanShoot)
            {
                // If the max force has been exceeded and the shell hasn't yet been launched...
                if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
                {
                    // ... use the max force and launch the shell.
                    m_CurrentLaunchForce = m_MaxLaunchForce;
                    Fire();
                }
                // Otherwise, if the fire button has just started being pressed...
                else if (Input.GetButtonDown(m_FireButton))
                {
                    // ... reset the fired flag and reset the launch force.
                    m_Fired = false;
                    m_CurrentLaunchForce = m_MinLaunchForce;

                    // Change the clip to the charging clip and start it playing.
                    m_ShootingAudio.clip = m_ChargingClip;
                    m_ShootingAudio.Play();
                }
                // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
                else if (Input.GetButton(m_FireButton) && !m_Fired)
                {
                    // Increment the launch force and update the slider.
                    m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                    m_AimSlider.value = m_CurrentLaunchForce;
                }
                // Otherwise, if the fire button is released and the shell hasn't been launched yet...
                else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
                {
                    // ... launch the shell.
                    m_ShootingAudio.Stop();
                    Fire();
                }
            }

            else if (m_CanShoot)
            {

                // If the max force has been exceeded and the shell hasn't yet been launched...
                if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
                {
                    // ... use the max force and launch the shell.
                    m_CurrentLaunchForce = m_MaxLaunchForce;
                    Fire();
                }
                // Otherwise, if the fire button has just started being pressed...
                else if (m_SerialController.m_Shoots[m_PlayerTeamID] && !m_Charging)
                {
                    // ... reset the fired flag and reset the launch force. Set the charging flag.
                    m_Fired = false;
                    m_CurrentLaunchForce = m_MinLaunchForce;
                    m_Charging = true;

                    // Change the clip to the charging clip and start it playing.
                    m_ShootingAudio.clip = m_ChargingClip;
                    m_ShootingAudio.Play();
                }
                // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
                else if (m_SerialController.m_Shoots[m_PlayerTeamID] && m_Charging && !m_Fired)
                {
                    // Increment the launch force and update the slider.
                    m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                    m_AimSlider.value = m_CurrentLaunchForce;
                }
                // Otherwise, if the fire button is released and the shell hasn't been launched yet...
                else if (!m_SerialController.GetComponent<SerialController>().m_Shoots[m_PlayerTeamID] && !m_Fired)
                {
                    //Set the charging flag.
                    m_Charging = false;
                    // ... launch the shell.
                    Fire();
                }
            }
        }
        else if (tag == "B" || tag == "C")
        {
            if (m_Controller == "Keyboard" && m_CanShoot)
            {
                // If the fire button is held and not on cooldown
                if (Input.GetButton(m_FireButton))
                {
                    //Fire a bullet
                    Fire();
                }
            }
            else if (m_CanShoot)
            {
                // If the fire button is held and not on cooldown
                if (m_SerialController.m_Shoots[m_PlayerTeamID] && m_CanShoot)
                {
                    //Fire a bullet
                    Fire();
                }
            }
        }
        if (m_Cooldown < 0f)
        {
            m_CanShoot = true;
            m_Cooldown = 0f;
        }
        else if (m_Cooldown > 0f)
        {
            m_Cooldown -= Time.deltaTime;
            if (m_Cooldown == 0f)
            {
                m_Cooldown = -1f;
            }
        }
        if (m_RocketCooldown < 0f)
        {
            if (m_RocketNumber < 17)
            {
                m_RocketNumber += 1;
            }
            m_RocketCooldown = 1.4f;
        }
        else if (m_RocketCooldown > 0f)
        {
            m_RocketCooldown -= Time.deltaTime;
            if (m_RocketCooldown == 0f)
            {
                m_RocketCooldown = -1f;
            }
        }
    }


    private void Fire()
    {
        // Set the fired flag so only Fire is only called once.

        m_CanShoot = false;
        m_Fired = true;

        if (tag == "A")
        {
            m_Cooldown = 0.8f;
            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance =
                Instantiate(m_Projectile, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

            // Reset the launch force and charging flag.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_Charging = false;
        }

        else if (tag == "B")
        {
            m_Cooldown = 0.15f;
            // Create an instance of the bullet and store a reference to it's rigidbody.
            Rigidbody bulletInstance =
                Instantiate(m_Projectile, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Set the bullet's velocity to the launch force in the fire position's forward direction.
            bulletInstance.velocity = 30f * m_FireTransform.up;

            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();

        }


        else if (tag == "C")
        {
            m_Cooldown = 0.3f;
            if (m_RocketNumber > 0)
            {
                m_RocketNumber -= 1;
                // Create an instance of the rocket and store a reference to it's rigidbody.
                Rigidbody rocketInstance = Instantiate(m_Projectile, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
                // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
                Collider[] colliders = Physics.OverlapSphere(transform.position, 40, m_TankMask);
                //Initialize list of enemies
                List<Transform> enemies = new List<Transform> { };
                // Go through all the colliders...
                for (int i = 0; i < colliders.Length; i++)
                {
                    // If it is an enemy
                    if (colliders[i].GetComponent<TankMovement>().m_PlayerNumber % 2 != m_PlayerNumber % 2)
                    {
                        enemies.Add(colliders[i].GetComponent<Transform>());
                    }
                }
                // Find nearest
                Transform bestTarget = null;
                float closestDistanceSqr = Mathf.Infinity;
                Vector3 currentPosition = transform.position;
                foreach (Transform potentialTarget in enemies)
                {
                    Vector3 directionToTarget = potentialTarget.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = potentialTarget;
                    }
                }

                if (bestTarget)
                {
                    // Create a  placeholder transform
                    Transform tempTransform = rocketInstance.GetComponent<Transform>();
                    // Point in direction of target
                    tempTransform.LookAt(bestTarget);
                    // Clamp the angle between -45 and 45 degrees of the cannon
                    float clampAngle = Mathf.Clamp(tempTransform.eulerAngles.y, m_FireTransform.eulerAngles.y - 45f, m_FireTransform.eulerAngles.y + 45f);
                    // Ensure angle is between 0 and 360
                    clampAngle = (clampAngle >= 360) ? clampAngle - 360 : clampAngle;
                    clampAngle = (clampAngle < 0) ? clampAngle + 360 : clampAngle;
                    // Instanciate rocket instance
                    rocketInstance.GetComponent<Transform>().eulerAngles = new Vector3(tempTransform.eulerAngles.x, clampAngle, 0f);
                    // Set the rocket's velocity to the launch force in the fire position's forward direction.
                    rocketInstance.velocity = 30f * rocketInstance.GetComponent<Transform>().forward;
                }
                else
                {
                    // Set the rocket's velocity to the launch force in the fire position's forward direction.
                    rocketInstance.velocity = 40f * m_FireTransform.forward;
                }
                // Change the clip to the firing clip and play it.
                m_ShootingAudio.clip = m_FireClip;
                m_ShootingAudio.Play();
            }
        }


    }
}
