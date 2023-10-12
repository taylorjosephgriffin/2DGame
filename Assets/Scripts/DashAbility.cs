//using UnityEngine;
//using System.Collections;

//public class DashAbility : MonoBehaviour
//{

//  public DashState dashState;
//  public float dashTimer;
//  public float maxDash = 20f;

//  public Vector2 savedVelocity;

//  Rigidbody2D rigidbody2d;
//  PlayerControls controls;
//  bool isDashKeyDown = false;

//  void Awake()
//  {
//    controls = new PlayerControls();
//    controls.Gameplay.Enable();
//    controls.Gameplay.Dash.performed += ctx => isDashKeyDown = true;
//    rigidbody2d = GetComponent<Rigidbody2D>();
//  }


//  void Update()
//  {
//    switch (dashState)
//    {
//      case DashState.Ready:
//        if (isDashKeyDown)
//        {
//          Debug.Log("DASH KEY DOWN");
//          savedVelocity = rigidbody2d.velocity;
//          rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x * 3f, rigidbody2d.velocity.y);
//          dashState = DashState.Dashing;
//        }
//        break;
//      case DashState.Dashing:
//        dashTimer += Time.deltaTime * 3;
//        if (dashTimer >= maxDash)
//        {
//          Debug.Log("DASHING");
//          dashTimer = maxDash;
//          rigidbody2d.velocity = savedVelocity;
//          dashState = DashState.Cooldown;
//        }
//        break;
//      case DashState.Cooldown:
//        dashTimer -= Time.deltaTime;
//        if (dashTimer <= 0)
//        {
//          Debug.Log("DONE DASHING");
//          dashTimer = 0;
//          dashState = DashState.Ready;
//        }
//        break;
//    }
//  }
//}

//public enum DashState
//{
//  Ready,
//  Dashing,
//  Cooldown
//}