using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering.Universal;

namespace PlayDecalVFX
{
public class PlayVFX : MonoBehaviour
{
    private DecalProjector _decal;
    private Material _mat;
    private VisualEffect _effect;

    private float _anticipation;
    private float _dissapation;
    private AnimationCurve _showIndicatorCurve;
    private AnimationCurve _dissolveCurve;
    private float _startTime;
    private float _lifetime;

    private bool _effectPlayed;
    // Start is called before the first frame update
    void Start()
    {
        //Assign variables
        _effect = gameObject.GetComponent<VisualEffect>();
        _effect.Play();
        _decal = gameObject.GetComponentInChildren<DecalProjector>();
        _mat = _decal.material;
        _anticipation = _effect.GetFloat("Anticipation");
        _dissapation = _effect.GetFloat("Dissapation");
        _startTime = Time.time;
        _showIndicatorCurve = _effect.GetAnimationCurve("IndicatorCurve");
        _dissolveCurve = _effect.GetAnimationCurve("DissolveCurve");

        //Apply properties to decal shader
        _mat.SetTexture("_Texture2D", _effect.GetTexture("IndicatorTexture"));
        _mat.SetColor("_BrightColor", _effect.GetVector4("BrightColor"));
        _mat.SetColor("_DarkColor", _effect.GetVector4("DarkColor"));
        _mat.SetInt("_UseLUT", _effect.GetBool("UseLUT") ? 1 : 0);
        _mat.SetTexture("_LUT", _effect.GetTexture("LUT"));

        //Apply diameter to decal
        _decal.size = new Vector3(_effect.GetFloat("Diameter") * 1.15f, _effect.GetFloat("Diameter") * 1.15f, _effect.GetFloat("Diameter") * 1.15f);

    }

    // Update is called once per frame
    void Update()
    {
        //calc lifetime depending on effect
        if(_effect.name == "PlantHealDecalPrefab(Clone)")
        {
            _lifetime = (Time.time - _startTime) * (1 / (_anticipation + _dissapation));
        }
        else
        {
            _lifetime = (Time.time - _startTime) * (1 / _anticipation);
        }
        
        
        //Animate Properties
        _mat.SetFloat("_SHowIndicator", _showIndicatorCurve.Evaluate(_lifetime));
        _mat.SetFloat("_Dissolve", _dissolveCurve.Evaluate(_lifetime));


        // Register PlayStart
        if (_effect.aliveParticleCount > 0 && !_effectPlayed)
        {
            _effectPlayed = true;
        }
        //Delete Game Object after Playing
        if (_effect.aliveParticleCount == 0 && _effectPlayed && _lifetime > _anticipation + _dissapation)
        {
            Destroy(gameObject);
        }
        
        
    }
}
}
