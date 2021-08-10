using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerConst {

    // the gravitational force pulling the player down each frame
    public const float GRAVITY = 5f;

    // the forces applied when holding a direction
    public const float SNEAKING_ACCEL = 20f;
    public const float SNEAKING_DECEL = 10f;
    public const float SNEAKING_MAX_SPEED = 10f;

    // jump height/speed
    public const float JUMP_ACCEL = 10f;
    // max jump hold time
    public const float MAX_JUMP_HOLD_TIME = .2f;

}
