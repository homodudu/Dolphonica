/******************************************************************************

- FlyerLaser [modified class]

Added enemy triggers and "hit" audio.

*******************************************************************************/

using System.Collections;
using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Flyer
{
    // This script controls the behaviour of each laser instance.
    public class FlyerLaser : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 500f;              // The speed each laser moves forward at.
        [SerializeField] private float m_LaserLifeDuration = 5f;    // How long the laser lasts before it's returned to it's object pool.


        private Rigidbody m_RigidBody;                              // Reference to the rigidbody of the laser.
        private bool m_Hit;                                         // Whether the laser has hit something.
        private DolphinScript dolphin;

        public ObjectPool ObjectPool { private get; set; }          // The object pool the laser belongs to.


        private void Awake()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            dolphin = FindObjectOfType<DolphinScript>();
        }

        private void Start()
        {

        }


        private void Update()
        {
            m_RigidBody.MovePosition(m_RigidBody.position + transform.forward * m_Speed * Time.deltaTime);
        }


        private void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.name.Contains("FlyerShark"))
            {

                Shark shark = other.gameObject.GetComponent<Shark>();

                shark.Hit();

                // The laser has hit something.
                m_Hit = true;

                dolphin.hitAudio.Play();

                // Return the laser to the object pool.
                ObjectPool.ReturnGameObjectToPool(gameObject);



            }

             if (other.gameObject.name.Contains("FlyerAsteroid"))
            {

                Asteroid asteroid = other.gameObject.GetComponent<Asteroid>();
                asteroid.Hit();

                // The laser has hit something.
                m_Hit = true;

                dolphin.hitAudio.Play();

                // Return the laser to the object pool.
                ObjectPool.ReturnGameObjectToPool(gameObject);

            }

            if (other.gameObject.name.Contains("FlyerOctopus"))
            {

                Pearl pearl = other.gameObject.GetComponent<Pearl>();
                pearl.Hit();

                dolphin.hitAudio.Play();

                // The laser has hit something.
                m_Hit = true;

                // Return the laser to the object pool.
                ObjectPool.ReturnGameObjectToPool(gameObject);

            }

            if (other.gameObject.name.Contains("FlyerUrchin"))
            {

                Myst myst = other.gameObject.GetComponent<Myst>();
                myst.Hit();

                dolphin.hitAudio.Play();

                // The laser has hit something.
                m_Hit = true;

                // Return the laser to the object pool.
                ObjectPool.ReturnGameObjectToPool(gameObject);

            }

        }


        private IEnumerator Timeout()
        {
            // Wait for the life time of the laser.
            yield return new WaitForSeconds (m_LaserLifeDuration);

            // If the laser hasn't hit something return it to the object pool.
            if (!m_Hit)
			    ObjectPool.ReturnGameObjectToPool (gameObject);
        }


        public void Restart()
        {
            // At restart the laser hasn't hit anything.
            m_Hit = false;

            // Start the lifetime timeout.
            StartCoroutine (Timeout ());
        }
    }
}
