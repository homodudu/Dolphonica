/******************************************************************************

- AudioCue [original class based on Asteroid scipt]


1) Causes the particle effect prefab to emit whenever a beat is detected.
2) Particle will instantiate in random position of a circle's circumference.
3) Creates concentric circle visual effect.


*******************************************************************************/

﻿using System;
using System.Collections;
using UnityEngine;
using VRStandardAssets.Common;
using Random = UnityEngine.Random;

namespace VRStandardAssets.Flyer
{

    public class AudioCue : MonoBehaviour

    {
        [SerializeField] private BeatEventScript m_BeatEvent;
        [SerializeField] private GameObject m_CuePrefab;                    // Reference to partcile effect.
        [SerializeField] private Transform m_Cam;                           // Reference to the camera's position.

        [SerializeField] private float m_SpawnZoneDistance = 500f;          // The distance from the camera of the spawn spheres.
        [SerializeField] private float m_SpawnZoneRadius = 500f;            // The distance from the camera of the spawn spheres.

        [SerializeField] private int m_SpawnNumber = 1;                     // The number of spawns per instance.

        private const float k_RemovalDistance = 10f;                       // How far behind the camera the object must be before it is removed.

        int counter;

        public void Update()
        {
            StartCoroutine(SpawnPrefab());
            StartCoroutine(DestroyPrefab());
        }

        IEnumerator SpawnPrefab()
        {
            //Func<bool> isNotBeat = new Func<bool>(() => !m_BeatEvent.isBeat);
            GameObject audioSpawn;


            if (m_BeatEvent.counter == 1) //Ensure that it only executes once per frame.
            {

                for (int i = 0; i < m_SpawnNumber; i++) //Particle effect will instantantiate on circle circumference.
                {
                    Vector3 randomCircle = new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f);
                    Vector3 spawnPosition = m_Cam.position + Vector3.forward * m_SpawnZoneDistance + randomCircle.normalized * m_SpawnZoneRadius;
                    audioSpawn = Instantiate(m_CuePrefab, spawnPosition, transform.rotation);

                }

                yield return null;
            }

        }

        IEnumerator DestroyPrefab()
        {
            GameObject[] cuePrefabArray = GameObject.FindGameObjectsWithTag("Cue");

            foreach (var cuePrefab in cuePrefabArray)
            {
                if (cuePrefab.transform.position.z < m_Cam.position.z - k_RemovalDistance)
                {
                    Destroy(cuePrefab);
                }
            }

           yield return new WaitForSeconds (1f);

        }
    }

}
