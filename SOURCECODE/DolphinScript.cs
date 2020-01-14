/******************************************************************************

- DolphinScript [original class]

Character contoller class.

1) Contains methods to switch dolphin colour.
2) Contains methods for crashes, collisions and and invulnarabilty.
3) Contains methods for Oculus Touch Control implentation.
4) Contains methods for Barrel roll animation.


*******************************************************************************/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace VRStandardAssets.Flyer {

    public class DolphinScript : MonoBehaviour
    {
        public float rollTime;
        float crashTime;
        public bool hasCrashed;
        bool isLeftSpinReset;
        bool isRightSpinReset;
        float axisModifier;
        float theta;
        public bool isStickReset;
        public GameObject blueDolphin;
        public GameObject greenDolphin;
        public bool isRayGreen;
        public bool isInvincible;
        public FlyerMovementController movementController;
        public AudioMixer musicAudioMixer;
        public Metronome metronome;
        int initCutoffFrequency;
        public float crashSpeed;
        public Animator dolphinAnim;
        public AudioSource[] ringAudioArray;
        public AudioSource hitAudio;
        public AudioSource swimSound;
        public int ringAudioIndex = 0;


        string[] collisonObjectString = { "FlyerShark", "FlyerAsteroid", "FlyerUrchin", "FlyerOctopus"};

        // Start is called before the first frame update
        void Start()
        {
            rollTime = 0.5f;
            crashTime = 2f;
            axisModifier = 0.001f;
            isLeftSpinReset = true;
            isRightSpinReset = true;
            blueDolphin.GetComponent<SkinnedMeshRenderer>().enabled = false;
            greenDolphin.GetComponent<SkinnedMeshRenderer>().enabled = true;
            isRayGreen = true;
            initCutoffFrequency = 22000;



        }

        // Update is called once per frame
        void Update()
        {
            //crashTime = Mathf.Lerp(2.0f, 0.5f, (float)metronome.level/5);

            swimSound.panStereo = Input.GetAxis("LS Horizontal");

            dolphinAnim.SetFloat("SwimSpeed", Mathf.SmoothStep(0.5f, 1.0f, metronome.level * 0.1f));

            OVRInput.Update();

            theta = transform.rotation.eulerAngles.z;

            CheckStickReset();

            CheckBarrelRollInput();

            StartCoroutine(ResetBanking());

            if (hasCrashed && !isInvincible)
            {
                StartCoroutine(Crash());
                StartCoroutine(VibrateCrash());
            }
        }

        private IEnumerator VibrateRing()
        {
            OVRInput.SetControllerVibration(0.5f, 0.1f, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0.5f, 0.1f, OVRInput.Controller.LTouch);
            yield return new WaitForSeconds(0.03f);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
        }

        private IEnumerator VibrateCrash()
        {

            OVRInput.SetControllerVibration(0.05f, 0.1f, OVRInput.Controller.RTouch);
            OVRInput.SetControllerVibration(0.05f, 0.1f, OVRInput.Controller.LTouch);
            yield return null;

            if (!hasCrashed)
            {
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            }
 ;
        }

        private void OnTriggerEnter(Collider other)
        {

            foreach (var collisionObject in collisonObjectString)
            {
                hasCrashed |= other.gameObject.name.Contains(collisionObject) && !isInvincible;
                if (hasCrashed)
                {
                           hitAudio.Play();
                }

            }

            if (other.gameObject.name.Contains("Gate"))
            {
                ringAudioArray[ringAudioIndex].Play();
                ringAudioIndex += 1;
                StartCoroutine(VibrateRing());
            }

            if (ringAudioIndex > ringAudioArray.Length-1)
            {
                ringAudioIndex = 0;
            }



            hasCrashed |= other.tag.Equals("Green Ring") && !isRayGreen && !isInvincible;

            hasCrashed |= other.tag.Equals("Blue Ring") && isRayGreen && !isInvincible;
        }


        IEnumerator BarrelRoll(int direction)
        {
            float startRotation = 0;
            float endRotation = startRotation + 360.0f;
            float t = 0.0f;
            while (t < rollTime)
            {
                isLeftSpinReset = false;
                isRightSpinReset = false;

                t += Time.deltaTime;
                float zRotation = Mathf.SmoothStep(startRotation, endRotation, t / rollTime) % 360.0f;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, direction * zRotation);
                yield return null;

            }

            isLeftSpinReset = true;
            isRightSpinReset = true;

        }

        public IEnumerator Crash()
        {

            float startRotation = 0;
            float endRotation = startRotation + 360.0f;
            float t = 0.0f;

            while (t < crashTime)
            {

                t += Time.deltaTime;
                float x = Mathf.SmoothStep(startRotation, endRotation, t / crashTime) % 360.0f;


                crashSpeed = Mathf.Clamp(75f * Mathf.Cos(x*Mathf.Deg2Rad), 0, 75f);

                musicAudioMixer.SetFloat("Lowpass", Mathf.Clamp(initCutoffFrequency * Mathf.Cos(x * Mathf.Deg2Rad), 500, initCutoffFrequency));

                yield return null;
            }
            hasCrashed = false;
        }

        IEnumerator ResetBanking()
        {
            if (isStickReset && (transform.rotation.eulerAngles.z < -0.1 || transform.rotation.eulerAngles.z > 0.1))
            {
                float zRotation = Mathf.Lerp(theta, 0, 1);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, zRotation);
                yield return null;
            }

        }

        void CheckStickReset()
        {
            if (Input.GetAxis("LS Horizontal") > -axisModifier && Input.GetAxis("LS Horizontal") < axisModifier &&
                Input.GetAxis("LS Vertical") > -axisModifier && Input.GetAxis("LS Vertical") < axisModifier)
            {
                isStickReset = true;
            }
            else isStickReset = false;
        }

        void CheckBarrelRollInput()
        {
            if ((Input.GetButtonDown("L Trigger") || OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) && isLeftSpinReset)
            {
                StartCoroutine(BarrelRoll(1));
                SwapColour();
            }
            else
            if ((Input.GetButtonDown("R Trigger") || OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) && isRightSpinReset)
            {
                StartCoroutine(BarrelRoll(-1));
                SwapColour();
            }
        }

        public void SwapColour()
        {
            if (blueDolphin.GetComponent<SkinnedMeshRenderer>().enabled)
            {
                greenDolphin.GetComponent<SkinnedMeshRenderer>().enabled = true;
                blueDolphin.GetComponent<SkinnedMeshRenderer>().enabled = false;
                isRayGreen = true;
            }
            else if (greenDolphin.GetComponent<SkinnedMeshRenderer>().enabled)
            {
                greenDolphin.GetComponent<SkinnedMeshRenderer>().enabled = false;
                blueDolphin.GetComponent<SkinnedMeshRenderer>().enabled = true;
                isRayGreen = false;
            }
        }


    }

}
