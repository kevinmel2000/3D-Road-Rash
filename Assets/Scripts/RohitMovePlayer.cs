﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RohitMovePlayer : MonoBehaviour
{
    private CharacterController controller;

    private float speed = 12.0f;
    private float horizontalSpeed = 2.5f;
    private float speedMultiplier = 5.0f;
    private Vector3 moveVector;
    private float verticalVelocity = 0.0f;
    private float gravity = 12.0f;
    public Boolean isDead = false;
    private float animationDuration = 3;
    private CultureInfo ci= new CultureInfo("en-US");
    private Vector3 moveDirection = Vector3.zero;
    public float jumpSpeed = 8.0f;
    private GameObject lastGameObjectHit;
    private string lastScene;

    // Start is called before the first frame update
    void Start()
    {
        this.lastScene = SceneManager.GetActiveScene().name;
        controller = GetComponent<CharacterController>();
        lastGameObjectHit = new GameObject();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            Debug.Log("MovePlayer: Is Dead");
            SceneManager.LoadScene("GameOver");
        }
        if (Time.time < animationDuration)
        {
            controller.Move(Vector3.forward * speed * Time.deltaTime);
            return;
        }
        moveVector = Vector3.zero;

        if (controller.isGrounded)
        {
            // Player is on the ground
            verticalVelocity = -0.5f;
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, 0.0f);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection = moveDirection * speed;
            if (Input.GetButton("Jump"))
            {

                moveDirection.y = jumpSpeed;
                controller.Move(moveDirection * Time.deltaTime);
            }

        }
        else
        {
            // PlayerMotor is KeyNotFoundException on the ground
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // X - Left Right movement
        moveVector.x = Input.GetAxisRaw("Horizontal") * horizontalSpeed;

        // Y - Up Down movement
        moveVector.y = verticalVelocity;

        // Z - Forward Backword movement
        moveVector.z = speed;
        moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);

        controller.Move(moveDirection * Time.deltaTime * 1.9f);

        controller.Move(moveVector * Time.deltaTime);

        if (transform.position.y < -10)
        {
            Death();
        }

        // Calling the wall remover function
        // GameObject.Find("ObstacleManager").GetComponent<PlaceObstacles>().RemoveWall(transform.position.z);

    }


    // On collider hit
    // Called every time our player collides with any object
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((!hit.gameObject.name.StartsWith("Tile", true, ci)) && HitCheckAllDirections(hit))
        {
            Debug.Log("Collided with: " + hit.gameObject.name);

            if (hit.gameObject.name == "Obstacle_Sphere(Clone)" 
                || hit.gameObject.name == "Obstacle_Cube(Clone)" 
                || hit.gameObject.name == "Obstacle_Cylinder(Clone)")
            {
                // Sphere hit
                // GetComponent<RohitPlayerState>().CyclePowerUp();
                
                if(hit.gameObject != lastGameObjectHit)
                {
                    String textureName = GetComponent<RohitScoresCalculations>().AddSum(hit.gameObject);
                }
                
                //addValueText.text = textureName;
                //addValueText.transform.position = this.transform.position;
                LeanTween.move(hit.gameObject, new Vector3(-10,20,this.transform.position.z + 10), 0.5f);
                //Material hitObjectMaterial = hit.gameObject.GetComponent<MeshRenderer>().material;


                Color color = hit.gameObject.GetComponent<MeshRenderer>().material.color;
                color.a = 0.5f;
                hit.gameObject.GetComponent<MeshRenderer>().material.color = color;
                
                //Debug.Log("in health reduction if");
                //Destroy(hit.gameObject);
                

            }
            else if(hit.gameObject.name.StartsWith("Wall", true, ci) && hit.gameObject != lastGameObjectHit)
            {
                // Wall is hit
                // Debug.Log("Wall hit with value: " + hit.gameObject.GetComponent<Renderer>().material.mainTexture.name);

                float diff = 0.0f;
                int currentSum = GetComponent<RohitScoresCalculations>().getCurrentSum();
                int nextTarget = GetComponent<RohitScoresCalculations>().getNextTarget();
                diff = Math.Abs(currentSum - nextTarget);
                GetComponent<RohitScoresCalculations>().displayTargetAchieved(diff);
                GetComponent<RohitHealthCalculation>().ReduceHealth(diff*5.0f);
                GetComponent<RohitScoresCalculations>().IncrementTargetIndex();
                hit.gameObject.SetActive(false);
                //Destroy(hit.gameObject);
            }
            //else if (hit.gameObject.name == "Obstacle_Cube(Clone)")
            //{
            //    // Cube hit
            //    GetComponent<RohitPlayerState>().SkatePowerUp();
            //    Destroy(hit.gameObject);
            //}
            //else if (hit.gameObject.name == "Obstacle_Cylinder(Clone)")
            //{
            //    // reduce the  health
            //    Destroy(hit.gameObject);
            //    GetComponent<RohitHealthCalculation>().ReduceHealth(30.0f);

            //}
            //else if (hit.gameObject.name.StartsWith("Obstacle_Health", true, ci))
            //{
            //    // reduce the  health
            //    Destroy(hit.gameObject);
            //    GetComponent<RohitHealthCalculation>().IncreaseHealth(40.0f);

            //}
            else
            {
                // Death();
            }
            lastGameObjectHit = hit.gameObject;
        }

    }

    private void Death()
    {
        this.lastScene = SceneManager.GetActiveScene().name;
        isDead = true;
        GetComponent<RohitScoresCalculations>().OnDeathScoreStops();
        // Destroy(this);
    }

    public void IncreaseSpeedByVal(int difficultLevel)
    {
        speed = difficultLevel * speedMultiplier;
    }

    public void UpdateSpeedByPowerup(float levelSpeed)
    {
        speed = levelSpeed;
    }

    private bool HitCheckAllDirections(ControllerColliderHit hit)
    {
        return ((hit.point.z > transform.position.z + 0.01f) ||
                (hit.point.z < transform.position.z - 0.01f) ||
                (hit.point.x > transform.position.x + 0.01f) || 
                (hit.point.x < transform.position.x - 0.01f) || 
                (hit.point.y > transform.position.y + 0.01f) ||
                (hit.point.y < transform.position.y - 0.01f));
    }
    public void onReset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
    public void onJump()
    {
        if (isDead || !controller.isGrounded)
        {
            return;
        }
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection = moveDirection * speed;
        
        moveDirection.y = jumpSpeed;
        controller.Move(moveDirection * Time.deltaTime*1.9f);
        
    }


    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public string GetLastScene()
    {
        return this.lastScene;
    }
}
