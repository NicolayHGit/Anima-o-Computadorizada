using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem _fastBullet;
    [SerializeField] private ParticleSystem _sparks;
    [SerializeField] private ParticleSystem _fog;

    private bool _fogActive = true;


    private void Update()
    {
        if(_fogActive)
            _fog.transform.Rotate(Vector3.up * Time.deltaTime * 10f); //apply rotation to fog particle system


        if (Input.GetKeyDown(KeyCode.A))
        {
            FogStop();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            RestartBullets();
        }
    }

    private void FogStop() //Stop Fog Particle
    {
        _fogActive = false;
        _fog.Stop();
    }

    private void RestartBullets() //Restart Bullets
    {
        _fastBullet.Stop();
        _fastBullet.Play();
    }
}
