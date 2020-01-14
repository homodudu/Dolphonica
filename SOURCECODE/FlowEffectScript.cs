/******************************************************************************

- FlowEffectScript [original script]

1) Adds a pencil sketch post processing effect.
2) FlowEffect coroutine is actived when flow meter reaches 100.
3) Background changes from black to white sinusoidally.
4) Vertical re-orchestration of music layers driven by same sine function.

*******************************************************************************/

﻿using UnityEngine;
using System.Collections;
using VRStandardAssets.Utils;
using VRStandardAssets.Common;
using UnityEngine.Audio;

namespace VRStandardAssets.Flyer
{
    [ExecuteInEditMode]
    public class FlowEffectScript : MonoBehaviour
    {

        public Material postProcessMaterial;
        public Camera m_Camera;
        RenderTexture myRenderTexture;

        //const float fConversionFactor = 0.069f;
        //const float fConversionOffset = 0.17f;
        //public float fConversionStart = 0.5f;

        //public float testColor;
        //public float testFlow;
        public float flowFadeSpeed = 5;
        public float flowEffectDuration = 5;
        public FlyerLaserController flyerLaserController;
        public AudioMixer audioMixer;
        public DolphinScript dolphin;
        public GameObject bluePlanet;
        public GameObject redPlanet;
        public bool isFlow;
        public float colorThreshold;

        private void Start()
        {
            postProcessMaterial.SetFloat("_ColorThreshold", 0f);
            bluePlanet.SetActive(true);
            redPlanet.SetActive(false);
        }

        private void OnEnable()
        {
            //m_Camera.forceIntoRenderTexture = false;
        }

        private void Update() //If the flow meter reaches 100 start flow effect.
        {
            if (flyerLaserController.currentFlow>=100f)
            {
                StartCoroutine(FlowEffect());
            }

        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, postProcessMaterial);
        }

        private IEnumerator FlowEffect()
        {

            isFlow = true;

            dolphin.isInvincible = true;
            bluePlanet.SetActive(false);
            redPlanet.SetActive(true);

            float startRotation = 0;
            float endRotation = startRotation + 180f;
            float t = 0.0f;


            while (t < flowEffectDuration)
            {


                t += Time.deltaTime;

                float x = Mathf.SmoothStep(startRotation, endRotation, t / flowEffectDuration);
                float sinFunc = Mathf.Sin(x * Mathf.Deg2Rad);

                //Use a scaled sine function to fade between black and white backgrounds.
                colorThreshold = Mathf.Clamp(flowFadeSpeed * sinFunc, 0f, 4.8f);
                postProcessMaterial.SetFloat("_ColorThreshold", colorThreshold);


                //Use a scaled sine function to vertically re-orchestrate the "flow" and "main" themes.
                audioMixer.SetFloat("FlowVolume", Mathf.Clamp(300f * sinFunc - 80f, -80f, -10));
                audioMixer.SetFloat("MusicVolume", Mathf.Clamp(-100f * sinFunc, -80f, 0));

                yield return null;

            }

            isFlow = false;

            dolphin.isInvincible = false;

            bluePlanet.SetActive(true);
            redPlanet.SetActive(false);

            flyerLaserController.currentFlow = 0; // Reset flow meter once effect has ended. 

        }

    }

}
