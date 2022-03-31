﻿using UnityEngine;

[RequireComponent(typeof(EnemyAIStateMachine))]
public class SeekingAI : MonoBehaviour
{
    private EnemyAIStateMachine enemyAI;
    private Vector2 target, nextTarget;


    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIStateMachine>();

        // Turn off by default
        enabled = false;
    }

    private void FixedUpdate()
    {
        // query targeting system
        (target, nextTarget) = enemyAI.Targeting.TrackBarge();

        // instruct navigation system
        if (target != Vector2.zero && nextTarget != Vector2.zero)
        {
            enemyAI.Navigation.ApplySteer(target, nextTarget);
        }
        else
        {
            // something else
        }
    }
}