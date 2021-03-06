﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShipController : MonoBehaviour {
    public int shotCounter = 0;
    public static int weaponsLoad = 50;
    public static int score = 0;
    public static int health = 50;
    public static int goal;
    public float speed = 10f;

    public float moveSpeed = 10f;
    private float mouseMultiplier = 1.5f;

    private Rigidbody2D rb;

    public Text txtScore;
    public Text txtAmmo;
    public Text txtLives;
    public Text txtLevel;
    public Text txtLevelEnd;
    private bool endText = false;

    public Transform bulletSpawn;
    public GameObject bulletPrefab;

    public AudioSource bulletFire;
    public AudioSource bulletCollision;
    public AudioSource rockSound;
    public AudioSource enemyCollision;
    public AudioSource over;
    public AudioSource win;

    private void Awake()
    {
        txtLevelEnd.text = " ";
    }

    void Start () {
        rb = gameObject.GetComponent <Rigidbody2D>();
        GameController.instance.spawn = true;

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                score = 0;
                health = 10;
                goal = 10;
                break;
            case 1:
                //score = score;
                health = 50;
                goal = 25; //cumulative
                weaponsLoad = 99;
                endText = true;
                break;
            default:
                break;
        }

        txtScore.text = "Score: " + score;
        txtAmmo.text = "Ammo: " + (weaponsLoad - shotCounter);
        txtLives.text = "Health: " + health;
        Time.timeScale = 1;
        Object.Destroy(txtLevel, 3f);
    }

    void FixedUpdate() {

        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        float mouseH = Input.GetAxis("Mouse X");
        float mouseV = Input.GetAxis("Mouse Y");

        if (moveH != 0f || moveV != 0f)
        {
            Vector2 motion = new Vector2(moveH, moveV);
            rb.AddForce(motion * moveSpeed);
            rb.mass = rb.mass * 0.9999f;
        }
        else if (mouseH != 0f || mouseV != 0f)
        {
            Vector2 motion = new Vector2(mouseH * mouseMultiplier, mouseV * mouseMultiplier);
            rb.AddForce(motion * moveSpeed);
            rb.mass = rb.mass * 0.9999f;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        if (Input.GetMouseButtonDown(1))
        {
            GameController.instance.PlayerDead();
        }

        txtScore.text = "Score: " + score;
        txtLives.text = "Health: " + health;

        if (score >= goal)  //end of level
        {
            GameController.instance.MuteBG();
            GameController.instance.spawn = false;
            txtLevelEnd.text = "Level Complete\nYour Final Score is: " + score.ToString();

            if (endText)
            {
                txtLevelEnd.text += "\nPress (q) to quit";
                win.Play();
            } else
            {
                txtLevelEnd.text += "\nPress (n) for next level";
            }
            Time.timeScale = 0;
        }

        if (health <= 0)    //player dead
        {
            GameController.instance.MuteBG();
            GameController.instance.spawn = false;
            bulletFire.mute = true;
            bulletCollision.mute = true;
            over.Play();

            if (txtLevel != null) txtLevel.text = "";
            txtLevelEnd.text = "Game Over\nYour Final Score is: " + score.ToString();
            txtLevelEnd.text += "\nPress (r) to continue";

            Time.timeScale = 0;
            GameController.instance.PlayerDead();
        }
    }

    void Fire()
    {
        if (shotCounter < weaponsLoad)
        {
            bulletFire.Play();
            shotCounter++;
            txtAmmo.text = "Ammo: " + (weaponsLoad - shotCounter);

            GameObject bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
            Vector2 motion = new Vector2(10f, 0f) * 10f;
            bullet.GetComponent<Rigidbody2D>().AddForce(motion * (3 + SceneManager.GetActiveScene().buildIndex));

            //Debug.Log("Firing\n");
            Destroy(bullet, 10f);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("ShipController collision " + other.tag);

        if (other.tag.Equals("enemy"))
        {
            health--;
            enemyCollision.Play();
        }

        if (other.tag.Equals("enemybullet"))
        {
            //bulletCollision = other.GetComponent<AudioSource>();  //if different things had different audio
            bulletCollision.Play();
            Destroy(other.gameObject);
            health--;
        }

        if (other.tag.Equals("space")) //stop player from fleeing
        {
            Vector2 current = rb.velocity;
            rb.velocity = Vector3.zero;
            rb.AddForce(current * -3f);
        }

        if (other.tag.Equals("asteroid"))
        {
            rockSound.Play();
            health = health - (int) (other.GetComponent<Rigidbody2D>().mass * 10f);
        }
    }

    public static void UpdateScore(int amount)
    {
        score += amount;
    }

    public static void UpdateHealth(int amount)
    {
        health += amount;
    }

    void OnBecameInvisible()    //kill you for leaving the screen
    {
        Debug.Log("SC Invisibile " + this.tag);
        health = 0;
    }

    IEnumerator EndLevel(float time) //not used, was trying to delay
    {
        yield return new WaitForSecondsRealtime(time);
    }

}