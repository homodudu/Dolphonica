/******************************************************************************

- Metronome [original class]


1) Uses beat from "metronome" audio track to calculate bars of music.
2) Bars of music are used to determine and display current level.
3) Esacalating tempo of metronome audio is used to scale dolphin speed.


*******************************************************************************/

﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Flyer
{

    public class Metronome : MonoBehaviour
    {
        [SerializeField] private BeatEventScript m_BeatEvent;
        [SerializeField] private Text m_CurrentLevel;
        [SerializeField] private EnvironmentController m_EnvironmentController;
        [SerializeField] private FlyerMovementController m_MovementController;
        [SerializeField] private DolphinScript m_Dolphin;

        public int counter;
        public float level;

        private float initialSpeed = 70f;
        public float maxSpeed;


        // Start is called before the first frame update
        void Start()
        {
            counter = 0;
            level = 1;
            m_CurrentLevel.text = "Level: ";
            m_MovementController.m_Speed = initialSpeed;

        }

        // Update is called once per frame
        void Update()
        {
            StartCoroutine(UpdateLevel());

        }



        IEnumerator UpdateLevel()
        {
            if (m_BeatEvent.counter == 1)
            {
                counter += 1;
            }



            switch (counter)
            {
                // Hilbert Number Sequence to determine start of new musical section. Where x = yn+1. (y = length of music section).

                case 1:
                    level = 1;
                    break;
                case 65:
                    level = 2;
                    break;
                case 129:
                    level = 3;
                    break;
                case 193:
                    level = 4;
                    break;
                case 257:
                    level = 5;
                    break;
                case 321:
                    level = 6;
                    break;
                case 385:
                    level = 7;
                    break;
                case 449:
                    level = 8;
                    break;
                case 513:
                    level = 9;
                    break;
                case 577:
                    level = 10;
                    break;

            }


            if (counter > 0)
            {
                // Update the score text.
                m_CurrentLevel.text = "Level: " + (int)level;
            }

            if (!m_Dolphin.hasCrashed)
            {
                m_MovementController.m_Speed = Mathf.Lerp(initialSpeed, maxSpeed, level * 0.1f);
            }
            else m_MovementController.m_Speed = m_Dolphin.crashSpeed;



            yield return null;
        }
    }

}
