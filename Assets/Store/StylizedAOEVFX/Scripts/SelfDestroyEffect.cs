using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace VFXSelfDestroy 
{

public class SelfDestroyEffect : MonoBehaviour
{
    private VisualEffect _effect;
    private bool _effectPlayed;
    // Start is called before the first frame update
    void Start()
    {
        _effect = gameObject.GetComponent<VisualEffect>();
        _effect.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(_effect.aliveParticleCount > 0 && !_effectPlayed)
        {
            _effectPlayed = true;
        }

        if(_effect.aliveParticleCount == 0 && _effectPlayed)
        {
            Destroy(gameObject);
        }
        
    }
}
}
