/******************************************************************************

- Whale [modified from Asteroid class]

Controls each whale in the scene.


*******************************************************************************/

﻿using System;
using UnityEngine;
using VRStandardAssets.Common;
using Random = UnityEngine.Random;

namespace VRStandardAssets.Flyer
{
    // This class controls each asteroid in the flyer scene.
    public class Whale : MonoBehaviour
    {
        public event Action<Whale> OnWhaleRemovalDistance;    // This event is triggered when it is far enough behind the camera to be removed.
        public event Action<Whale> OnWhaleHit;                // This event is triggered when the asteroid is hit either the ship or a laser.



        [SerializeField] private int m_PlayerDamage = 0;           // The amount of damage the asteroid will do to the ship if it hits.
        [SerializeField] private int m_Score = 0;                  // The amount added to the score when the asteroid hits either the ship or a laser.


        private Rigidbody m_RigidBody;                              // Reference to the asteroid's rigidbody, used to move and rotate it.
        private FlyerHealthController m_FlyerHealthController;      // Reference to the flyer's health script, used to damage it.
        private GameObject m_Flyer;                                 // Reference to the flyer itself, used to determine what was hit.
        private Transform m_Cam;                                    // Reference to the camera so this can be destroyed when it's behind the camera.
        private float m_Speed;                                      // How fast asteroid will move towards the camera.


        private const float k_RemovalDistance = 500f;                // How far behind the camera the asteroid must be before it is removed.


        public int Score { get { return m_Score; } }


        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();

            m_FlyerHealthController = FindObjectOfType<FlyerHealthController>();
            m_Flyer = m_FlyerHealthController.gameObject;

            m_Cam = Camera.main.transform;
        }


        private void Start()
        {
            // Set a random scale for the asteroid.

        }


        private void Update()
        {


            // If the shark is far enough behind the camera and something has subscribed to OnAsteroidRemovalDistance call it.
            if (transform.position.z < m_Cam.position.z - k_RemovalDistance)
                if (OnWhaleRemovalDistance != null)
                    OnWhaleRemovalDistance(this);
        }


        private void OnTriggerEnter(Collider other)
        {
            // Only continue if the asteroid has hit the flyer.
            if (other.gameObject != m_Flyer)
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
            OnWhaleRemovalDistance = null;
            OnWhaleHit = null;
        }


        public void Hit()
        {
            // Add to the score.
            SessionData.AddScore(m_Score);

            // If OnAsteroidHit has any subscribers call it.
            if (OnWhaleHit != null)
                OnWhaleHit(this);
        }
    }
}
