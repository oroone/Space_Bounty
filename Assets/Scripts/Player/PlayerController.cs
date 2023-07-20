using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed;
    private int desiredLane = 1; // 0 = Left, 1 = Middle, 2 = Right
    public float laneDistance = 4; //Distance between two lanes
    public float jumpForce;
    public float gravity = -20;
    public Animator animator;
    public float maxSpeed;
    private bool isSliding = false;
    private float originalSpeed;
    private float speedMultiplier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalSpeed = forwardSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        direction.z = forwardSpeed;
        speedMultiplier = originalSpeed / forwardSpeed;
        
        if (!PlayerManager.isGameStarted)
            return;

        //Increase speed while user is playing
        if(forwardSpeed < maxSpeed)
        {
            forwardSpeed += 0.5f * Time.deltaTime;
        }
        
        animator.SetBool("isGameStarted", true);

        //JUMP
        if (controller.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Jump();
                animator.SetBool("isGrounded", false);
            }
        }
        else
        {
            direction.y += gravity * Time.deltaTime;
            animator.SetBool("isGrounded", true);
        }
        //SLIDE
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isSliding)
        {
            StartCoroutine(Slide());
        }

        //Get inputs for which lane we should be in
        //Move RIGHT
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            desiredLane++;
            if (desiredLane == 3)
            {
                desiredLane = 2;
            }
        }
        //Move LEFT
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            desiredLane--;
            if (desiredLane == -1)
            {
                desiredLane = 0;
            }
        }

        //Calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;

        if(desiredLane == 0)
        {
            targetPosition += Vector3.left * laneDistance;
        }else if(desiredLane == 2)
        {
            targetPosition += Vector3.right * laneDistance;
        }

        //transform.position = targetPosition;
        if (transform.position == targetPosition)
            return;
        Vector3 diff = targetPosition - transform.position;
        Vector3 moveDir = diff.normalized * 25 * Time.deltaTime;
        if (moveDir.sqrMagnitude < diff.sqrMagnitude)
            controller.Move(moveDir);
        else
            controller.Move(diff);
    }

    private void FixedUpdate()
    {
        if (!PlayerManager.isGameStarted)
            return;

        controller.Move(direction * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.transform.tag == "Obstacle")
        {
            PlayerManager.gameOver = true;
        }
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);
        controller.center = new Vector3(0, -0.5f, 0);
        controller.height = 1;
        yield return new WaitForSeconds(1.1f * speedMultiplier);
        controller.center = new Vector3(0, 0, 0);
        controller.height = 2;
        animator.SetBool("isSliding", false);
        isSliding = false;
    }

}
