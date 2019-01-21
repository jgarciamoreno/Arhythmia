using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParticleDisable : MonoBehaviour {

    public enum DisableType { ANIMATION, TIME }
    public DisableType disableType;

    public float disableTime;

    private ParticleSystem ps;
    private RectTransform trans;
    private float lifeTime;
    private float size;
	
	void Start ()
    {
        ps = GetComponent<ParticleSystem>();
        trans = GetComponent<RectTransform>();
	}

    void OnEnable() 
    {
        lifeTime = 0;
        size = 1f;
    }
	
	void Update ()
    {
        switch (disableType)
        {
            case DisableType.ANIMATION:
                if (ps.isStopped)
                    gameObject.SetActive(false);
                break;
            case DisableType.TIME:
                lifeTime += Time.deltaTime;
                if (lifeTime > disableTime)
                    gameObject.SetActive(false);

                size += 0.2f;
                trans.localScale = new Vector2(size, size);
                break;
        }

	}
}
