using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

    public Animation anim;
    public AnimationClip[] Animations;

    private IAnimationState mainAnimationState;
    private IAnimationState additiveAnimationState;
    private IdleAnimationState IdleAnimationState;
    private JumpAnimationState JumpAnimationState;
    private AttackAnimationState AttackAnimationState;
    private BlockAnimationState BlockAnimationState;
    private EvadeAnimationState EvadeAnimationState;
    private HitAnimationState HitAnimationState;
    private DieAnimationState DieAnimationState;
    private DanceAnimationState DanceAnimationState;
    private InteractAnimationState InteractAnimationState;
    private MovementBlendTree MovementBlendTree;

    private void Awake()
    {
        IdleAnimationState      = new IdleAnimationState(this);
        JumpAnimationState      = new JumpAnimationState();
        AttackAnimationState    = new AttackAnimationState();
        BlockAnimationState     = new BlockAnimationState();
        EvadeAnimationState     = new EvadeAnimationState();
        HitAnimationState       = new HitAnimationState();
        DieAnimationState       = new DieAnimationState();
        DanceAnimationState     = new DanceAnimationState();
        InteractAnimationState  = new InteractAnimationState();
        MovementBlendTree       = new MovementBlendTree(this);

        SetupAnimations();

        mainAnimationState = IdleAnimationState;
    }

    private void OnEnable()
    {
        MusicController.OnAudioStart += MusicController_OnAudioStart;
    }
    private void OnDisable()
    {
        MusicController.OnAudioStart -= MusicController_OnAudioStart;
    }

    private void MusicController_OnAudioStart()
    {
        //RhythmTimer = 60f / MusicController.bpm;
    }

    private void SetupAnimations()
    {
        List<AnimationClip> acs = new List<AnimationClip>();
        string prevAnimName = "";
        string prevSubAnimNumber = "";
        for (int i = 0; i < Animations.Length; i++)
        {
            AnimationClip ac = Animations[i];
            anim.AddClip(ac, ac.name);

            string[] animName = ac.name.Split('_');
            if (prevAnimName == "")
                prevAnimName = animName[0];
            if (prevSubAnimNumber == "")
                prevSubAnimNumber = animName[1].Substring(0, 1);

            if (acs != null)
            {
                bool toFinish = false;
                if (i == Animations.Length - 1)
                {
                    acs.Add(ac);
                    toFinish = true;
                }

                if ((animName[0] != prevAnimName || animName[1].Substring(0, 1) != prevSubAnimNumber) || toFinish)
                {
                    switch (prevAnimName)
                    {
                        case "Idle":
                            IdleAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Walk":
                            //WalkAnimations.AddAnimations(acs.ToArray());
                            break;
                        case "Run":
                            MovementBlendTree.AddAnimations(acs.ToArray());
                            break;
                        case "Jump":
                            JumpAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Attack":
                            AttackAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Block":
                            BlockAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Evade":
                            EvadeAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Hit":
                            HitAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Die":
                            DieAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Dance":
                            DanceAnimationState.AddAnimations(acs.ToArray());
                            break;
                        case "Interact":
                            InteractAnimationState.AddAnimations(acs.ToArray());
                            break;
                    }

                    prevAnimName = animName[0];
                    prevSubAnimNumber = animName[1].Substring(0, 1);
                    acs.Clear();
                }
            }

            acs.Add(ac);
        }
    }



    private void LateUpdate()
    {
        mainAnimationState.Play();
    }
    
    public void Play(string animation, float fade)
    {
        anim.CrossFade(animation, fade);
    }

    public void AdditivePlay(string animation, float weight)
    {
        anim[animation].blendMode = AnimationBlendMode.Additive;
        anim.CrossFade(animation);
    }

    public void Move(float amount)
    {
        
        if (amount > 0)
            mainAnimationState = MovementBlendTree;
        else
            mainAnimationState = IdleAnimationState;
    }
}