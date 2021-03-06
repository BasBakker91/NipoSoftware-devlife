﻿using System;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(Health))]
public class Player : MonoBehaviour
{
    public float maxJumpVelocity = 6f;
    public float gravity = -4f;
    public float rebounceStrenght = 4f;

    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;

    public Vector3 startPosition;

    public static Player Instance;

    private Health health;
    private Controller2D controller;
    private SpriteRenderer spriteRenderer;

    private float moveSpeed = 6f;

    private Vector3 velocity;
    private Vector2 directionalInput;

    private float velocityXSmoothing;

    public Vector3 facingDirection => spriteRenderer.flipX ? Vector3.left : Vector3.right;

    private void Start()
    {
        Instance = this;

        startPosition = transform.position;

        health = GetComponent<Health>();
        controller = GetComponent<Controller2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        health.Die += OnDeath;
    }

    public void OnDeath()
    {
        transform.position = startPosition;
        GameManager.Instance.Reset();
    }

    private void Update()
    {
        CalculateVelocity();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0f;
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        input.y = 0;
        directionalInput = input;

        if (directionalInput.x != 0)
        {
            spriteRenderer.flipX = directionalInput.x < 0;
        }
    }
    
    public void OnJumpInputDown()
    {
        if (controller.collisions.below)
        {
            velocity.y = maxJumpVelocity;
            GameManager.Instance.audioManager.PlayJumpSound();
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            health.TakeDamage(100);

            velocity.y = maxJumpVelocity;
            velocity.x = (-Vector3.Normalize(collision.gameObject.transform.position - gameObject.transform.position) * rebounceStrenght).x;
        }
        else if (collision.gameObject.tag == "EnemyWeakness")
        {
            velocity.y = maxJumpVelocity / 2f;
            var bugHealth = collision.gameObject.GetComponent<Health>();
            bugHealth.TakeDamage(100);
        }
    }

    private void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));
        velocity.y += gravity * Time.deltaTime;
    }
}
