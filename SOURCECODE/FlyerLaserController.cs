/******************************************************************************

- FlyerLaserController [modified class]



1) Changed lasers to vortex/echo rings.
2) Shot colour changes to match dolphin's current colour.
2) Added a "recharge" gauge and shooting mechanic to deter button mashing.
3) Added "flow" calculation system from score and combo data.
4) Added panning to shot "audio".
5) Music tempo data taken from Metronome class sets the spawn frequencies.


*******************************************************************************/

﻿using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;
using VRStandardAssets.Common;
using UnityEngine.UI;

namespace VRStandardAssets.Flyer
{
    // This script handles getting the laser instances from
    // the object pool and firing them.
    public class FlyerLaserController : MonoBehaviour
    {
        [SerializeField] private VRInput m_VRInput;                     // Reference to the VRInput so when the fire button is pressed it can be handled.
        [SerializeField] private FlyerGameController m_GameController;  // Reference to the game controller so firing can be limited to when the game is running.
        [SerializeField] private ObjectPool m_LaserObjectPool;          // Reference to the object pool the lasers belong to.
        [SerializeField] private Transform m_LaserSpawnPos;         // The positions the lasers should spawn from.
        [SerializeField] private AudioSource m_LaserAudio;              // The audio source that should play firing sounds.

        [SerializeField] private Image[] m_SonarChargeBar;                     // Reference to the image used as a health bar.

        [SerializeField] private Text m_CurrentScore;               // Reference to the Text component that will display the user's score.
        [SerializeField] private Text m_CurrentCombo;               // EDIT Reference to the Text component that will display the user's combo.
        [SerializeField] private Text m_CurrentFlow;

        [SerializeField] private DolphinScript m_Dolphin;

        [SerializeField] private int m_LaserChargeTime = 2;

        Renderer m_Renderer;
        private Color c_Green = new Color32(21, 91, 40, 255);
        private Color c_Blue = new Color32(21, 91, 180, 255);
        private bool isFireable = true;
        private bool hasIncrementedFlow;
        private Material pencilMaterial;
        public int currentFlow;
        int tempScore = 0;
        int tempCombo = 0;

        int audioIndex = 0;

        private void OnEnable()
        {
            m_VRInput.OnDown += HandleDown;
        }


        private void OnDisable()
        {
            m_VRInput.OnDown -= HandleDown;
        }


        private void HandleDown()
        {
            // If the game isn't running return.
            if (!m_GameController.IsGameRunning)
                return;

            if (isFireable)
            {
                // Fire laser from each position.
                StartCoroutine(FireLaser(m_LaserSpawnPos));
            }


        }

        private void Start()
        {
            m_SonarChargeBar[0].color = c_Green;
            m_SonarChargeBar[1].color = c_Green;
            m_SonarChargeBar[0].fillAmount = 1f;
            m_SonarChargeBar[1].fillAmount = 1f;
        }

        private void Update()
        {
            StartCoroutine(UpdateSessionData());

            if (m_Dolphin.isRayGreen)
            {
                m_SonarChargeBar[0].color = c_Green;
                m_SonarChargeBar[1].color = c_Green;
            }
            else
            {
                m_SonarChargeBar[0].color = c_Blue;
                m_SonarChargeBar[1].color = c_Blue;
            }
        }


        IEnumerator UpdateSessionData()
        {

            SetFlow();

            // Update the score text.
            m_CurrentScore.text = "Score: " + SessionData.Score;

            // Update the combo text.
            m_CurrentCombo.text = "Combo: " + SessionData.Combo;

            // Update the flow text.
            m_CurrentFlow.text = "Flow: " + Mathf.Clamp(currentFlow, 0, 100) + "%";

            yield return null;
        }

        IEnumerator FireLaser(Transform gunPos)
        {
            isFireable = false;

            m_SonarChargeBar[0].fillAmount = 0;
            m_SonarChargeBar[1].fillAmount = 0;

            // Get a laser from the pool.
            GameObject laserGameObject = m_LaserObjectPool.GetGameObjectFromPool();

            // Set it's position and rotation based on the gun positon.
            laserGameObject.transform.position = gunPos.position;
            laserGameObject.transform.rotation = gunPos.rotation;

            if (m_Dolphin.isRayGreen)
            {
                laserGameObject.transform.GetChild(0).gameObject.SetActive(false);
                laserGameObject.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                laserGameObject.transform.GetChild(0).gameObject.SetActive(true);
                laserGameObject.transform.GetChild(1).gameObject.SetActive(false);
            }



            // Find the FlyerLaser component of the laser instance.
            FlyerLaser flyerLaser = laserGameObject.GetComponent<FlyerLaser>();

            // Set it's object pool so it knows where to return to.
            flyerLaser.ObjectPool = m_LaserObjectPool;

            // Restart the laser.
            flyerLaser.Restart();

            // Play laser audio.
            m_LaserAudio.panStereo = Input.GetAxis("LS Horizontal");
            m_LaserAudio.Play();

            //Recharge shot before firing again.
            float startRotation = 0;
            float endRotation = startRotation + 90.0f;
            float t = 0.0f;
            while (t < m_LaserChargeTime)
            {

                t += Time.deltaTime;

                float x = Mathf.Lerp(startRotation, endRotation, t / m_LaserChargeTime) % 90.0f;


                m_SonarChargeBar[0].fillAmount = Mathf.Sin(x * Mathf.Deg2Rad);
                m_SonarChargeBar[1].fillAmount = Mathf.Sin(x * Mathf.Deg2Rad);

                yield return null;
            }

            isFireable = true;

            m_SonarChargeBar[0].fillAmount = 1f;
            m_SonarChargeBar[1].fillAmount = 1f;


        }

        private void SetFlow() // Calculate flow from score and combo data.
        {
            hasIncrementedFlow &= ((tempCombo == 0 || tempCombo == SessionData.Combo) && (tempScore == 0 || tempScore == SessionData.Score));

            if (!hasIncrementedFlow && ((SessionData.Score != 0 && SessionData.Score % 500 == 0) || SessionData.Combo != 0 && SessionData.Combo % 5 == 0))
            {
                StartCoroutine(IncrementFlow());

                tempCombo = SessionData.Combo;
                tempScore = SessionData.Score;
                hasIncrementedFlow = true;
            }

        }

        IEnumerator IncrementFlow() // Increment flow steadily over time rather than instantly.
        {
            for (int i = 0; i < 5; i++)
            {
                currentFlow += 1;
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void StopGame()
        {
            currentFlow = 0;
        }

    }
}
