/******************************************************************************

- EnvironmentController [modified class]

PEARL CLASS IS NOW OCTOPUS CLASS
MYST CLASS IS NOW URCHIN CLASS


1) Script originally contained code to spawn asteroids and asteroid explosions.
2) Script originally contained code to handle flyer and ring interactions.
3) Added shark, octopus, whale, urchin and fish interactions.
4) Spawn frequency now increases over time.
5) Music tempo data taken from Metronome class sets the spawn frequencies.


*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Common;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Flyer
{
    // This script handles the spawning and some of the
    // interactions of Rings and Asteroids with the flyer.
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private float m_AsteroidSpawnFrequency = 2f;       // The time between asteroids spawning in seconds.
        [SerializeField] private float m_SharkSpawnFrequency = 8f;       // The time between asteroids spawning in seconds.
        [SerializeField] private float m_RingSpawnFrequency = 4f;          // The time between rings spawning in seconds.
        [SerializeField] private float m_PearlSpawnFrequency = 32f;          // The time between rings spawning in seconds.
        [SerializeField] private float m_MystSpawnFrequency = 16f;       // The time between asteroids spawning in seconds.
        [SerializeField] private float m_WhaleSpawnFrequency = 64f;       // The time between asteroids spawning in seconds.
        [SerializeField] private int m_InitialAsteroidCount = 10;           // The number of asteroids present at the start.
        [SerializeField] private float m_AsteroidSpawnZoneRadius = 120f;    // The radius of the sphere in which the asteroids spawn.
        [SerializeField] private float m_RingSpawnZoneRadius = 50f;         // The radius of the sphere in which the rings spawn.
        [SerializeField] private float m_SharkSpawnZoneRadius = 60f;         // The radius of the sphere in which the sharks spawn.
        [SerializeField] private float m_PearlSpawnZoneRadius = 60f;         // The radius of the sphere in which the pearls spawn.
        [SerializeField] private float m_MystSpawnZoneRadius = 60f;         // The radius of the sphere in which the mysts spawn.
        [SerializeField] private float m_WhaleSpawnZoneRadius = 300f;         // The radius of the sphere in which the mysts spawn


        [SerializeField] private float m_SpawnZoneDistance = 500f;          // The distance from the camera of the spawn spheres.
        [SerializeField] private ObjectPool m_AsteroidObjectPool;           // The object pool that stores the asteroids.
        [SerializeField] private ObjectPool m_AsteroidExplosionObjectPool;  // The object pool that stores the expolosions made when asteroids are hit.
        [SerializeField] private ObjectPool m_RingObjectPool;               // The object pool that stores the rings.
        [SerializeField] private ObjectPool m_SharkObjectPool;               // EDIT - The object pool that stores the shark.
        [SerializeField] private ObjectPool m_SharkExplosionObjectPool;
        [SerializeField] private ObjectPool m_PearlObjectPool;
        [SerializeField] private ObjectPool m_PearlExplosionObjectPool;
        [SerializeField] private ObjectPool m_MystObjectPool;
        [SerializeField] private ObjectPool m_MystExplosionObjectPool;
        [SerializeField] private ObjectPool m_WhaleObjectPool;

        [SerializeField] private DolphinScript m_Dolphin;
        [SerializeField] private Metronome m_Metronome;
        [SerializeField] private Transform m_Cam;                           // Reference to the camera's position.




        private List<Ring> m_Rings;                                         // Collection of all the currently unpooled rings.
        private List<Asteroid> m_Asteroids;                                 // Collection of all the currently unpooled asteroids.
        private List<Shark> m_Sharks;                                        // Collection of all the currently unpooled sharks.
        private List<Pearl> m_Pearls;                                        // Collection of all the currently unpooled pearls.
        private List<Myst> m_Mysts;                                        // Collection of all the currently unpooled mysts.
        private List<Whale> m_Whales;                                        // Collection of all the currently unpooled mysts.


        private bool m_Spawning;                                            // Whether the environment should keep spawning rings and asteroids.
        private const float spawnFreqModifier = 0.3f;

        public void StartEnvironment()
        {
            // Create new empty lists for the rings and asteroids.
            m_Rings = new List<Ring>();
            m_Asteroids = new List<Asteroid>();
            m_Sharks = new List<Shark>();
            m_Pearls = new List<Pearl>();
            m_Mysts = new List<Myst>();
            m_Whales = new List<Whale>();




            // Spawn all the starting asteroids.
            for (int i = 0; i < m_InitialAsteroidCount; i++)
            {
                SpawnAsteroid();
            }


            //SpawnRing();

            // Restart the score and set the score's type to be FLYER
            SessionData.Restart();
            SessionData.SetGameType(SessionData.GameType.FLYER);

            // The environment has started so spawning can start.
            m_Spawning = true;

            // Start spawning asteroids and rings.
            StartCoroutine(SpawnAsteroidRoutine());
            StartCoroutine(SpawnRingRoutine());
            StartCoroutine(SpawnSharkRoutine());
            StartCoroutine(SpawnPearlRoutine());
            StartCoroutine(SpawnMystRoutine());
            StartCoroutine(SpawnWhaleRoutine());


        }


        public void StopEnvironment()
        {
            // The environment has stopped so spawning should no longer happen.
            m_Spawning = false;

            // While there are asteroids in the collection, remove the first asteroid.
            while (m_Asteroids.Count > 0)
            {
                HandleAsteroidRemoval(m_Asteroids[0]);
            }

            // While there are rings in the collection, remove the first ring.
            while (m_Rings.Count > 0)
            {
                HandleRingRemove(m_Rings[0]);
            }

            // While there are asteroids in the collection, remove the first asteroid.
            while (m_Sharks.Count > 0)
            {
                HandleSharkRemoval(m_Sharks[0]);
            }

            // While there are rings in the collection, remove the first ring.
            while (m_Pearls.Count > 0)
            {
                HandlePearlRemoval(m_Pearls[0]);
            }

            // While there are rings in the collection, remove the first ring.
            while (m_Mysts.Count > 0)
            {
                HandleMystRemoval(m_Mysts[0]);
            }

            // While there are rings in the collection, remove the first ring.
            while (m_Whales.Count > 0)
            {
                HandleWhaleRemoval(m_Whales[0]);
            }
        }

        private void SpawnWhale()
        {
            // Get an asteroid from the object pool.
            GameObject whaleGameObject = m_WhaleObjectPool.GetGameObjectFromPool();

            // Generate a position at a distance forward from the camera within a random sphere and put the asteroid at that position.
            Vector3 whalePosition = m_Cam.position + Vector3.forward * (m_SpawnZoneDistance + 500) + Random.insideUnitSphere.normalized * m_WhaleSpawnZoneRadius;
            whaleGameObject.transform.position = whalePosition;

            // Get the asteroid component and add it to the collection.
            Whale whale = whaleGameObject.GetComponent<Whale>();
            m_Whales.Add(whale);

            // Subscribe to the asteroids events.
            whale.OnWhaleRemovalDistance += HandleWhaleRemoval;

        }


        private IEnumerator SpawnWhaleRoutine()
        {
            yield return new WaitForSeconds(90f);
            do
            {
                SpawnWhale();
                yield return new WaitForSeconds(m_WhaleSpawnFrequency);
            }
            while (m_Spawning);
        }

        private void HandleWhaleRemoval(Whale whale)
        {
            // Now the ring has been removed, unsubscribe from the event.
            whale.OnWhaleRemovalDistance -= HandleWhaleRemoval;


            // Remove the ring from it's collection.
            m_Whales.Remove(whale);

            // Return the ring to its object pool.
            m_WhaleObjectPool.ReturnGameObjectToPool(whale.gameObject);
        }


        private void SpawnMyst()
        {
            // Get an asteroid from the object pool.
            GameObject mystGameObject = m_MystObjectPool.GetGameObjectFromPool();

            // Generate a position at a distance forward from the camera within a random sphere and put the asteroid at that position.
            Vector3 mystPosition = m_Cam.position + Vector3.forward * m_SpawnZoneDistance + Random.insideUnitSphere * m_MystSpawnZoneRadius;
            mystGameObject.transform.position = mystPosition;

            // Get the asteroid component and add it to the collection.
            Myst myst = mystGameObject.GetComponent<Myst>();
            m_Mysts.Add(myst);

            // Subscribe to the asteroids events.
            myst.OnMystRemovalDistance += HandleMystRemoval;
            myst.OnMystHit += HandleMystHit;

        }

        private IEnumerator SpawnMystRoutine()
        {
            yield return new WaitForSeconds(10f);
            do
            {
                SpawnMyst();
                yield return new WaitForSeconds(Mathf.Lerp(m_MystSpawnFrequency, m_MystSpawnFrequency * spawnFreqModifier, m_Metronome.level * 0.1f));
            }
            while (m_Spawning);
        }

        private void HandleMystHit(Myst myst)
        {

            // Remove the pearl when it's hit.
            HandleMystRemoval(myst);

            // Get an explosion from the object pool and put it at the asteroids position.
            GameObject explosion = m_MystExplosionObjectPool.GetGameObjectFromPool();
            explosion.transform.position = myst.transform.position;

            // Get the pearl explosion component and restart it.
            MystExplosion mystExplosion = explosion.GetComponent<MystExplosion>();
            mystExplosion.Restart();

            // Subscribe to the asteroid explosion's event.
            mystExplosion.OnExplosionEnded += HandleMystExplosionEnded;

        }

        private void HandleMystRemoval(Myst myst)
        {
            // Now the ring has been removed, unsubscribe from the event.
            myst.OnMystRemovalDistance -= HandleMystRemoval;


            // Remove the ring from it's collection.
            m_Mysts.Remove(myst);

            // Return the ring to its object pool.
            m_MystObjectPool.ReturnGameObjectToPool(myst.gameObject);
        }

        private void HandleMystExplosionEnded(MystExplosion explosion)
        {
            // Now the explosion has finished unsubscribe from the event.
            explosion.OnExplosionEnded -= HandleMystExplosionEnded;

            // Return the explosion to its object pool.
            m_MystExplosionObjectPool.ReturnGameObjectToPool(explosion.gameObject);
        }





        private void SpawnPearl()
        {

            // Get an asteroid from the object pool.
            GameObject pearlGameObject = m_PearlObjectPool.GetGameObjectFromPool();

            // Generate a position at a distance forward from the camera within a random sphere and put the asteroid at that position.
            Vector3 pearlPosition = m_Cam.position + Vector3.forward * m_SpawnZoneDistance + Random.insideUnitSphere * m_PearlSpawnZoneRadius;
            pearlGameObject.transform.position = pearlPosition;

            if (pearlGameObject.transform.position.z - m_Cam.position.z < 100)
            {
                pearlPosition.z = m_Cam.position.z;
            }

            // Get the asteroid component and add it to the collection.
            Pearl pearl = pearlGameObject.GetComponent<Pearl>();
            m_Pearls.Add(pearl);

            // Subscribe to the asteroids events.
            pearl.OnPearlRemovalDistance += HandlePearlRemoval;
            pearl.OnPearlHit += HandlePearlHit;
        }

        private IEnumerator SpawnPearlRoutine()
        {
            yield return new WaitForSeconds(30f);
            // While the environment is spawning, spawn an asteroid and wait for another one.
            do
            {
                SpawnPearl();
                yield return new WaitForSeconds(Mathf.Lerp(m_PearlSpawnFrequency, m_PearlSpawnFrequency * spawnFreqModifier, m_Metronome.level * 0.1f));
            }
            while (m_Spawning);
        }

        private void HandlePearlHit(Pearl pearl)
        {
            // Remove the pearl when it's hit.
            HandlePearlRemoval(pearl);

            // Get an explosion from the object pool and put it at the asteroids position.
            GameObject explosion = m_PearlExplosionObjectPool.GetGameObjectFromPool();
            explosion.transform.position = pearl.transform.position;

            // Get the pearl explosion component and restart it.
            PearlExplosion pearlExplosion = explosion.GetComponent<PearlExplosion>();
            pearlExplosion.Restart();

            // Subscribe to the asteroid explosion's event.
            pearlExplosion.OnExplosionEnded += HandlePearlExplosionEnded;

        }

        private void HandlePearlRemoval(Pearl pearl)
        {
            // Only one of HandleAsteroidRemoval and HandleAsteroidHit should be called so unsubscribe both.
            pearl.OnPearlRemovalDistance -= HandlePearlRemoval;
            pearl.OnPearlHit -= HandlePearlHit;

            // Remove the asteroid from the collection.
            m_Pearls.Remove(pearl);

            // Return the asteroid to its object pool.
            m_PearlObjectPool.ReturnGameObjectToPool(pearl.gameObject);
        }

        private void HandlePearlExplosionEnded(PearlExplosion explosion)
        {
            // Now the explosion has finished unsubscribe from the event.
            explosion.OnExplosionEnded -= HandlePearlExplosionEnded;

            // Return the explosion to its object pool.
            m_PearlExplosionObjectPool.ReturnGameObjectToPool(explosion.gameObject);
        }






        private void SpawnAsteroid()
        {

            // Get an asteroid from the object pool.
            GameObject asteroidGameObject = m_AsteroidObjectPool.GetGameObjectFromPool();

            // Generate a position at a distance forward from the camera within a random sphere and put the asteroid at that position.
            Vector3 asteroidPosition = m_Cam.position + Vector3.forward * m_SpawnZoneDistance + Random.insideUnitSphere * m_AsteroidSpawnZoneRadius;
            asteroidGameObject.transform.position = asteroidPosition;

            // Get the asteroid component and add it to the collection.
            Asteroid asteroid = asteroidGameObject.GetComponent<Asteroid>();
            m_Asteroids.Add(asteroid);

            // Subscribe to the asteroids events.
            asteroid.OnAsteroidRemovalDistance += HandleAsteroidRemoval;
            asteroid.OnAsteroidHit += HandleAsteroidHit;
        }


        private IEnumerator SpawnAsteroidRoutine()
        {
            // While the environment is spawning, spawn an asteroid and wait for another one.
            do
            {
                SpawnAsteroid();
                yield return new WaitForSeconds(Mathf.Lerp(m_AsteroidSpawnFrequency, m_AsteroidSpawnFrequency * spawnFreqModifier, m_Metronome.level * 0.1f));
            }
            while (m_Spawning);
        }

        private void HandleAsteroidHit(Asteroid asteroid)
        {
            // Remove the asteroid when it's hit.
            HandleAsteroidRemoval(asteroid);

            // Get an explosion from the object pool and put it at the asteroids position.
            GameObject explosion = m_AsteroidExplosionObjectPool.GetGameObjectFromPool();
            explosion.transform.position = asteroid.transform.position;

            // Get the asteroid explosion component and restart it.
            AsteroidExplosion asteroidExplosion = explosion.GetComponent<AsteroidExplosion>();
            asteroidExplosion.Restart();

            // Subscribe to the asteroid explosion's event.
            asteroidExplosion.OnExplosionEnded += HandleAsteroidExplosionEnded;
        }


        private void HandleAsteroidRemoval(Asteroid asteroid)
        {
            // Only one of HandleAsteroidRemoval and HandleAsteroidHit should be called so unsubscribe both.
            asteroid.OnAsteroidRemovalDistance -= HandleAsteroidRemoval;
            asteroid.OnAsteroidHit -= HandleAsteroidHit;

            // Remove the asteroid from the collection.
            m_Asteroids.Remove(asteroid);

            // Return the asteroid to its object pool.
            m_AsteroidObjectPool.ReturnGameObjectToPool(asteroid.gameObject);
        }


        private void HandleAsteroidExplosionEnded(AsteroidExplosion explosion)
        {
            // Now the explosion has finished unsubscribe from the event.
            explosion.OnExplosionEnded -= HandleAsteroidExplosionEnded;

            // Return the explosion to its object pool.
            m_AsteroidExplosionObjectPool.ReturnGameObjectToPool(explosion.gameObject);
        }




        private void SpawnShark()
        {

            // Get an shark from the object pool.
            GameObject sharkGameObject = m_SharkObjectPool.GetGameObjectFromPool();

            // Generate a position at a distance forward from the camera within a random sphere and put the asteroid at that position.
            Vector3 sharkPosition = m_Cam.position + Vector3.forward * m_SpawnZoneDistance + Random.insideUnitSphere * m_SharkSpawnZoneRadius;
            sharkGameObject.transform.position = sharkPosition;

            // Get the asteroid component and add it to the collection.
            Shark shark = sharkGameObject.GetComponent<Shark>();
            m_Sharks.Add(shark);

            // Subscribe to the asteroids events.
            shark.OnSharkRemovalDistance += HandleSharkRemoval;
            shark.OnSharkHit += HandleSharkHit;

        }

        private IEnumerator SpawnSharkRoutine()
        {
            yield return new WaitForSeconds(60f);
            // While the environment is spawning, spawn an asteroid and wait for another one.
            do
            {
                SpawnShark();
                yield return new WaitForSeconds(Mathf.Lerp(m_SharkSpawnFrequency, m_SharkSpawnFrequency * spawnFreqModifier, m_Metronome.level * 0.1f));
            }
            while (m_Spawning);

        }

        private void HandleSharkHit(Shark shark)
        {
            // Remove the asteroid when it's hit.
            HandleSharkRemoval(shark);

            // Get an explosion from the object pool and put it at the asteroids position.
            GameObject explosion = m_SharkExplosionObjectPool.GetGameObjectFromPool();
            explosion.transform.position = shark.transform.position;

            // Get the asteroid explosion component and restart it.
            SharkExplosion sharkExplosion = explosion.GetComponent<SharkExplosion>();
            sharkExplosion.Restart();

            // Subscribe to the asteroid explosion's event.
            sharkExplosion.OnExplosionEnded += HandleSharkExplosionEnded;

        }


        private void HandleSharkRemoval(Shark shark)
        {
            // Only one of HandleAsteroidRemoval and HandleAsteroidHit should be called so unsubscribe both.
            shark.OnSharkRemovalDistance -= HandleSharkRemoval;
            shark.OnSharkHit -= HandleSharkHit;

            // Remove the asteroid from the collection.
            m_Sharks.Remove(shark);

            // Return the asteroid to its object pool.
            m_SharkObjectPool.ReturnGameObjectToPool(shark.gameObject);
        }

        private void HandleSharkExplosionEnded(SharkExplosion explosion)
        {
            // Now the explosion has finished unsubscribe from the event.
            explosion.OnExplosionEnded -= HandleSharkExplosionEnded;

            // Return the explosion to its object pool.
            m_SharkExplosionObjectPool.ReturnGameObjectToPool(explosion.gameObject);
        }




        private void SpawnRing()
        {
            if (!m_Dolphin.hasCrashed)
            {
                // Get a ring from the object pool.
                GameObject ringGameObject = m_RingObjectPool.GetGameObjectFromPool();

                // Generate a position at a distance forward from the camera within a random sphere and put the ring at that position.
                Vector3 ringPosition = m_Cam.position + Vector3.forward * m_SpawnZoneDistance + Random.insideUnitSphere * m_RingSpawnZoneRadius;
                ringGameObject.transform.position = ringPosition;

                // Get the ring component, restart it and add it to the collection.
                Ring ring = ringGameObject.GetComponent<Ring>();
                ring.Restart();

                // Subscribe to the remove event.
                ring.OnRingRemove += HandleRingRemove;
            }



        }

        private IEnumerator SpawnRingRoutine()
        {
            // With an initial delay, spawn a ring and delay whilst the environment is spawning.
            do
            {
                SpawnRing();

                yield return new WaitForSeconds(Mathf.Lerp(m_RingSpawnFrequency, m_RingSpawnFrequency*spawnFreqModifier, m_Metronome.level* 0.1f));
            }
            while (m_Spawning);
        }

        private void HandleRingRemove(Ring ring)
        {

            // Now the ring has been removed, unsubscribe from the event.
            ring.OnRingRemove -= HandleRingRemove;

            // Remove the ring from it's collection.
            m_Rings.Remove(ring);

            // Return the ring to its object pool.
            m_RingObjectPool.ReturnGameObjectToPool(ring.gameObject);
        }
    }
}
