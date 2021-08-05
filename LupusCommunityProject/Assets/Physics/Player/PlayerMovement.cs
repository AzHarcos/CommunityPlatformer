using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    private float PlayerSpeedX;
    private float PlayerSpeedY;

    private float PlayerAccelX;


    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        float dt = Time.deltaTime;

        ProcessInput();
        UpdatePlayer(dt);
    }

    private void UpdatePlayer(float dt) {
        if (PlayerSpeedX < PlayerConst.sneakingMaxSpeed) {
            PlayerSpeedX += PlayerAccelX * dt;
        } else {
            PlayerSpeedX = PlayerConst.sneakingMaxSpeed;
        }

        transform.position += new Vector3(PlayerSpeedX * dt, PlayerSpeedY * dt, 0);
    }

    void ProcessInput () {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            PlayerAccelX = PlayerConst.SNEAKING_ACCEL;
        }
    }
}
