using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerConst {

    // the gravitational force pulling the player down each frame
    public const float GRAVITY = 5f;

    // the forces applied when crawling without the shell
    public const float CRAWL_ACCEL = 20f;
    public const float CRAWL_DECEL = 10f;
    public const float CRAWL_MAX_SPEED = 10f;

    // the forces applied when crawling with the shell
    public const float CRAWL_ACCEL_SHELL = 20f;
    public const float CRAWL_DECEL_SHELL = 20f;
    public const float CRAWL_MAX_SPEED_SHELL = 10f;
    
    // jump height/speed
    public const float JUMP_ACCEL = 10f;
    // max jump hold time
    public const float MAX_JUMP_HOLD_TIME = .2f;

}
