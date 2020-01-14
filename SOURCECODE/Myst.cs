/******************************************************************************

- Myst [modified from Asteroid class]

Controls each myst in the scene.

NOW URCHIN CLASS

If the dolphin will change colour if it collides with the urchin.

*******************************************************************************/

﻿using System;
using System.Collections;
using UnityEngine;
using VRStandardAssets.Common;
using Random = UnityEngine.Random;

namespace VRStandardAssets.Flyer
{
    // This class controls each myst in the flyer scene.
    public class Myst : MonoBehaviour
    {
        public event Action<Myst> OnMystRemovalDistance;    // This event is triggered when it is far enough behind the camera to be removed.
        public event Action<Myst> OnMystHit;                // This event is triggered when the myst is hit either the ship or a laser.



        [SerializeField] private int m_PlayerDamage = 0;           // The amount of damage the myst will do to the ship if it hits.
        [SerializeField] private int m_Score = 0;                  // The amount added to the score when the myst hits either the ship or a laser.

        private float m_Freq; // The maximum speed the myst will bob up and down.
        private float m_Dist;            // The maximum distance the myst will bob up and down.


        private Rigidbody m_RigidBody;                              // Reference to the myst's rigidbody, used to move and rotate it.
        private FlyerHealthController m_FlyerHealthController;      // Reference to the flyer's health script, used to damage it.
        private GameObject m_Flyer;                                 // Reference to the flyer itself, used to determine what was hit.
        private Transform m_Cam;                                    // Reference to the camera so this can be destroyed when it's behind the camera.
        private Vector3 m_RotationAxis;                             // The axis around which the myst will rotate.

        private DolphinScript m_Dolphin;

        private Vector3 m_Position1;
        private Vector3 m_Position2;

        private const float k_RemovalDistance = 500f;                // How far behind the camera the myst must be before it is removed.


        public int Score { get { return m_Score; } }



        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();

            m_FlyerHealthController = FindObjectOfType<FlyerHealthController>();
            m_Flyer = m_FlyerHealthController.gameObject;

            m_Dolphin = FindObjectOfType<DolphinScript>();

            m_Cam = Camera.main.transform;




        }


        private void Start()
        {
            m_Freq = Random.Range(0.1f, 0.2f);
            m_Dist = Random.Range(50f, 200f);
            m_Position1 = transform.position;
            m_Position2 = transform.position + Vector3.up * m_Dist;
        }


        private void Update()
        {
            m_RigidBody.position = Vector3.Lerp(m_Position1, m_Position2, Mathf.PingPong(Time.time * m_Freq, 1.0f));


            // If the myst is far enough behind the camera and something has subscribed to OnmystRemovalDistance call it.
            if (transform.position.z < m_Cam.position.z - k_RemovalDistance)

                if (OnMystRemovalDistance != null)
                    OnMystRemovalDistance(this);


        }


        private void OnTriggerEnter(Collider other)
        {
            // Only continue if the myst has hit the flyer.
            if (other.gameObject != m_Flyer || m_Dolphin.isInvincible)
                return;

            // Damage the flyer.
            m_FlyerHealthController.TakeDamage(m_PlayerDamage);

            m_Dolphin.SwapColour(); // Dolphin will change colour on collision.


            // If the damage didn't kill the flyer add to the score and call the appropriate events.
            if (!m_FlyerHealthController.IsDead)
                Hit();
        }


        private void OnDestroy()
        {
            // Ensure the events are completely unsubscribed when the myst is destroyed.
            OnMystRemovalDistance = null;
            OnMystHit = null;
        }


        public void Hit()
        {
            // Add to the score.
            SessionData.AddScore(m_Score);

            // If OnmystHit has any subscribers call it.
            if (OnMystHit != null)
                OnMystHit(this);
        }

    }
}
