/******************************************************************************

- Ring [modified]

1) Added new materials to player rings.
2) Rings now pulsate in time to the music.
3) Method to detect if the dolphin passes through same colour ring.
4) Updated ring scoring system.


Materials taken from M. Bieg (2018) Available at:
https://assetstore.unity.com/packages/vfx/particles/ultimate-vfx-26701
*******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Common;

namespace VRStandardAssets.Flyer
{
    // This script handles the behaviour of the gates
    // in the flyer scene including changing their colour
    // and adding to the player's score.
    public class Ring : MonoBehaviour
    {
        public event Action<Ring> OnRingRemove;


        [SerializeField] private int m_Score = 100;                         // The amount added to the player's score when the ring is activated.
        [SerializeField] private int m_Damage = 100;                         // The amount added to the player's score when the ring is activated.
        [SerializeField] private int m_MissDamage = 50;                         // The amount added to the player's score when the ring is activated.
        //[SerializeField] private AudioSource[] m_AudioSource;                // Reference to the audio source that plays a clip when the player activates the ring.
        [SerializeField] private Color m_BaseColor = Color.blue;            // The colour the ring is by defalt.
        [SerializeField] private Color m_ShipAlignedColor = Color.yellow;   // The colour the ring is when the ship is aligned with it.
        [SerializeField] private Color m_ActivatedColor = Color.green;      // The colour the ring is when it has been activated.

        [SerializeField] private GameObject m_sclera;                       // Edit
        [SerializeField] private GameObject m_myst;                       // Edit
        [SerializeField] private GameObject m_halo;                       // Edit

        private bool m_HasTriggered;
        private Transform m_Cam;
        private GameObject m_Flyer;
        private List<Material> m_Materials;
        private bool m_ShipAligned;
        private GameObject m_Dolphin;
        private bool m_hasMissed;
        private GameObject m_beatEvent;


        private const float k_RemovalDistance = 500f;
        private const float k_MissedDistance = 10f;

        FlyerHealthController m_health;

        // This property is used choose a colour for the ring based on the flyer's alignment.
        public bool ShipAligned
        {
            set
            {
                m_ShipAligned = value;

                // If this ring has already been triggered it should be the triggered colour so return.
                if (m_HasTriggered)
                    return;

                // Otherwise set the ring's colour based on whether the ship
                SetRingColour (m_ShipAligned ? m_ShipAlignedColor : m_BaseColor);
            }

            get { return m_ShipAligned; }
        }


        private void Awake()
        {
            // Create a list of materials and add the main material on each child renderer to it.
            m_Materials = new List<Material>();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                m_Materials.Add(renderers[i].material);
            }

            // Set references to the camera and flyer.
            m_Cam = Camera.main.transform;
            m_Flyer = GameObject.FindGameObjectWithTag ("Player");

            m_Dolphin = GameObject.FindGameObjectWithTag("Dolphin");

            m_beatEvent = GameObject.FindGameObjectWithTag("Beat Event Ring");

            m_health = m_Flyer.GetComponent<FlyerHealthController>();
        }


        private void Update()
        {
            bool isMissedConditions = !m_HasTriggered && !m_sclera.activeSelf && !m_hasMissed && !m_Dolphin.GetComponent<DolphinScript>().isInvincible;

             if ((transform.position.z < m_Cam.position.z - k_MissedDistance) && isMissedConditions)
            {
                SessionData.ResetCombo();
                m_health.TakeDamage(m_MissDamage);
                m_hasMissed = true;
            }

            // If the ring is far enough behind the camera and something is subscribed to OnRingRemove call it.
            if (transform.position.z < m_Cam.position.z - k_RemovalDistance)
                if (OnRingRemove != null)
                    OnRingRemove(this);

            if (m_beatEvent.GetComponent<BeatEventScript>().isBeat)
            {

                m_halo.SetActive(true);
                m_halo.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                m_halo.SetActive(false);
            }

        }

    
        private void OnTriggerEnter(Collider other)
        {
            // If this ring has already triggered or the ring has not collided with the flyer return.
            if (m_HasTriggered || other.gameObject != m_Flyer)
                return;

            // Otherwise the ring has been triggered.
            m_HasTriggered = true;

            // Play audio.
            //m_AudioSource[m_Dolphin.GetComponent<DolphinScript>().ringAudioIndex].Play();

            if (m_Dolphin.GetComponent<DolphinScript>().isRayGreen && gameObject.tag.Equals("Green Ring") || m_Dolphin.GetComponent<DolphinScript>().isInvincible)
            {
                // Add to the score.
                SessionData.AddScore(m_Score);
                SessionData.AddCombo(1);
                m_myst.SetActive(true);

            }
             if (!m_Dolphin.GetComponent<DolphinScript>().isRayGreen && gameObject.tag.Equals("Blue Ring") || m_Dolphin.GetComponent<DolphinScript>().isInvincible)
            {
                // Add to the score.
                SessionData.AddScore(m_Score);
                SessionData.AddCombo(1);
                m_myst.SetActive(true);

            }

            if (!m_Dolphin.GetComponent<DolphinScript>().isRayGreen && gameObject.tag.Equals("Green Ring") &&  !m_Dolphin.GetComponent<DolphinScript>().isInvincible)
            {
                m_health.TakeDamage(m_Damage);
                SessionData.ResetCombo();
                m_sclera.SetActive(true);
            }

            if (m_Dolphin.GetComponent<DolphinScript>().isRayGreen && gameObject.tag.Equals("Blue Ring") && !m_Dolphin.GetComponent<DolphinScript>().isInvincible)
            {
                m_health.TakeDamage(m_Damage);
                SessionData.ResetCombo();
                m_sclera.SetActive(true);
            }


            // Set the ring's colour.
            SetRingColour(m_ActivatedColor);


        }


        private void OnDestroy()
        {
            // Ensure the event is completely unsubscribed when the ring is destroyed.
            OnRingRemove = null;
        }


        public void Restart()
        {
            m_sclera.SetActive(false);
            m_myst.SetActive(false);

            // Reset the colour to it's original colour.
            SetRingColour (m_BaseColor);

            // The ring has no longer been triggered.
            m_HasTriggered = false;

            m_hasMissed = false;
        }


        private void SetRingColour (Color color)
        {
            // Go through all the materials and set their colour appropriately.
            for (int i = 0; i < m_Materials.Count; i++)
            {
                m_Materials[i].color = color;
            }
        }
    }
}
