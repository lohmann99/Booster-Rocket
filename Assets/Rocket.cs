using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour {

    Rigidbody rigidBody;
    AudioSource audioSource;

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 50f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip deathExplosion;
    [SerializeField] AudioClip successSound;

    [SerializeField] ParticleSystem engineParticle;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] ParticleSystem successParticle;

    bool collisionDisabled = false;

    enum State { Alive, Trancending, Dying}
    State state = State.Alive;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        if (state == State.Alive)
        {
            Rotate();
            Thrust(); 
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionDisabled = !collisionDisabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || collisionDisabled) { return; }

        switch (collision.gameObject.tag)
        {
            case "FRIENDLY":           
                break;
            case "Finish":
                StartFinishSqeuence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(deathExplosion);
        deathParticle.Play();
        Invoke("RestartGame", 1f);
    }

    private void StartFinishSqeuence()
    {
        state = State.Trancending;
        audioSource.Stop();
        audioSource.PlayOneShot(successSound);
        successParticle.Play();
        Invoke("LoadNextLevel", 1f);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int currentNextLevel;
        currentNextLevel = currentSceneIndex + 1;
        if (currentNextLevel == SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(currentNextLevel);
        }
        
    }

    private void Rotate()
    {
        rigidBody.freezeRotation = true; // Stop physics simulation.

        float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {            
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // resume physics simulation.
    }

    private void Thrust()
    {
        float thrustThisFrame = mainThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
        {
            rigidBody.AddRelativeForce(Vector3.up * thrustThisFrame);

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(mainEngine);
                engineParticle.Play();
            }
        }
        else
        {
            audioSource.Stop();
            engineParticle.Stop();
        }
    }
}
