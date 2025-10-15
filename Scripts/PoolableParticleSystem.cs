using UnityEngine;

namespace RAXY.Pooling
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PoolableParticleSystem : PoolableObject
    {
        ParticleSystem _particleSystem;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;

            var allParticles = GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in allParticles)
            {
                if (particle == _particleSystem)
                    continue;

                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var childMain = particle.main;
                childMain.duration = _particleSystem.main.duration;

                if (childMain.playOnAwake)
                {
                    particle.Play();
                }
            }
        }

        void OnParticleSystemStopped()
        {
            Release();
        }
    }
}

