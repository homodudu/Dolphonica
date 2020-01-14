/******************************************************************************

- Pearl [modified from Asteroid class]

Controls each pearl in the scene.

NOW OCTOPUS CLASS

Added "linger" coroutine.

*******************************************************************************/

﻿using System;
using System.Collections;
using UnityEngine;
using VRStandardAssets.Common;
using Random = UnityEngine.Random;

namespace VRStandardAssets.Flyer
{
    // This class controls each asteroid in the flyer scene.
    public class Pearl : MonoBehaviour
    {
        public event Action<Pearl> OnPearlRemovalDistance;    // This event is triggered when it is far enough behind the camera to be removed.
        public event Action<Pearl> OnPearlHit;                // This event is triggered when the asteroid is hit either the ship or a laser.



        [SerializeField] private int m_PlayerDamage = 0;           // The amount of damage the asteroid will do to the ship if it hits.
        [SerializeField] private int m_Score = 0;                  // The amount added to the score when the asteroid hits either the ship or a laser.
        [SerializeField] private float m_Speed = 5f;            // The maximum speed the asteroid will move towards the camera.
        [SerializeField] private float m_LingerDistance = 150f;
        [SerializeField] private float m_LingerTime = 5f;


        private DolphinScript m_Dolphin;
        private Rigidbody m_RigidBody;                              // Reference to the asteroid's rigidbody, used to move and rotate it.
        private FlyerHealthController m_FlyerHealthController;      // Reference to the flyer's health script, used to damage it.
        private FlyerMovementController m_FlyerMovementController;    // EDIT Reference to the flyer's movment script, used to damage it.
        private GameObject m_Flyer;                                 // Reference to the flyer itself, used to determine what was hit.
        private Transform m_Cam;                                    // Reference to the camera so this can be destroyed when it's behind the camera.
        private Vector3 m_RotationAxis;                             // The axis around which the asteroid will rotate.

        private bool hasLingered;



        private const float k_RemovalDistance = 500f;                // How far behind the camera the asteroid must be before it is removed.


        public int Score { get { return m_Score; } }


        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();

            m_FlyerHealthController = FindObjectOfType<FlyerHealthController>();
            m_FlyerMovementController = FindObjectOfType<FlyerMovementController>();

            m_Flyer = m_FlyerHealthController.gameObject;
            m_Dolphin = FindObjectOfType<DolphinScript>();
            m_Cam = Camera.main.transform;

        }


        private void Update()
        {

            if (transform.position.z - m_Cam.position.z > m_LingerDistance)
            {
                m_RigidBody.MovePosition(m_RigidBody.position - Vector3.forward * m_Speed * Time.deltaTime);
            }

            if (transform.position.z - m_Cam.position.z < m_LingerDistance && !hasLingered)
            {
                StartCoroutine(Linger());
            }

            // If the asteroid is far enough behind the camera and something has subscribed to OnAsteroidRemovalDistance call it.
            if (transform.position.z < m_Cam.position.z - k_RemovalDistance)

                if (OnPearlRemovalDistance != null)
                    OnPearlRemovalDistance(this);
        }


        private void OnTriggerEnter(Collider other)
        {
            // Only continue if the asteroid has hit the flyer.
            if (other.gameObject != m_Flyer || m_Dolphin.isInvincible)
                return;

            // Damage the flyer.
            m_FlyerHealthController.TakeDamage(m_PlayerDamage);


            // If the damage didn't kill the flyer add to the score and call the appropriate events.
            if (!m_FlyerHealthController.IsDead)
                Hit();
        }


        private void OnDestroy()
        {
            // Ensure the events are completely unsubscribed when the asteroid is destroyed.
            OnPearlRemovalDistance = null;
            OnPearlHit = null;
        }


        public void Hit()
        {
            // Add to the score.
            SessionData.AddScore(m_Score);

            // If OnAsteroidHit has any subscribers call it.
            if (OnPearlHit != null)
                OnPearlHit(this);
        }

        IEnumerator Linger() // Octopus will linger in front of the player for a defined time.
        {

            transform.Translate(Vector3.forward * Time.deltaTime * m_FlyerMovementController.m_Speed);

            yield return new WaitForSeconds (m_LingerTime);

            hasLingered = true;

            // Octopus will dash in the direction of the player.
            m_RigidBody.MovePosition(m_RigidBody.position - Vector3.forward * m_Speed * Time.deltaTime);

        }
    }
}
