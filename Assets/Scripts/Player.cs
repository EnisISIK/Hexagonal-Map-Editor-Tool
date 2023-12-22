using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    /*
    /    A first person character controller with gravity, acceleration, 
    /    jump abilities and capable of destroying and placing blocks.
    /    Blocks dont have colliders, because of that character controller 
    /    uses methods for checking block data.
    */

    [SerializeField]
    private bool isGrounded;

    private bool isSprinting;

    private Transform cam;
    private World world;

    [SerializeField]
    private float walkSpeed = 3f;
    [SerializeField]
    private float sprintSpeed = 6f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float gravity = -9.8f;
    [SerializeField]
    private float rayIncrement = 0.1f;
    [SerializeField]
    private float reach = 8f;
    [SerializeField]
    private bool focusOnEnable = true;

    [SerializeField]
    private float playerWidth = 0.15f;
    [SerializeField]
    private float playerHeight = 1.8f;

    [SerializeField]
    private Transform highlightBlock;
    [SerializeField]
    private Transform placeBlock;

    private float verticalMomentum = 0;
    private bool jumpRequest;

    private Vector3 velocity;
    private byte selectedBlockIndex = 1;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 moveVector = new Vector3(0f, 0f, 0f);

    [SerializeField]
    private bool isInteracting;

    private static bool IsFocused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = value == false;
        }
    }


    private void OnEnable()
    {
        if (focusOnEnable) IsFocused = true;
    }


    private void OnDisable()
    {
        IsFocused = false;
    }


    private void Start()
    {

        cam = GameObject.Find("Main Camera").transform;
        cam.localPosition = new Vector3(0, playerHeight, 0);
        world = GameObject.Find("World").GetComponent<World>();

    }


    private void FixedUpdate()
    {
        if (!world.DoesDataExists(PositionHelper.GetChunkFromVector3(PositionHelper.PixelToHex(cam.position)))) return;
        if (!IsFocused) return;

        CalculateVelocity();
        if (jumpRequest)
            Jump();

        UpdateTransform();
    }


    private void Update()
    {
        GetPlayerInputs();
        PlaceCursorBlocks();
    }


    private void Jump()
    {

        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }


    private void CalculateVelocity()
    {

        Vector3 direction = transform.TransformVector(moveVector.normalized);

        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        if (isSprinting)
            velocity = (direction) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = (direction) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && CheckFront) || (velocity.z < 0 && CheckBack))
            velocity.z = 0;
        if ((velocity.x > 0 && CheckRight) || (velocity.x < 0 && CheckLeft))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);

        moveVector = new Vector3(0, 0, 0);

    }


    private void GetPlayerInputs()
    {
        if (Input.GetKey(KeyCode.W))
            moveVector += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            moveVector += Vector3.back;
        if (Input.GetKey(KeyCode.D))
            moveVector += Vector3.right;
        if (Input.GetKey(KeyCode.A))
            moveVector += Vector3.left;
        if (Input.GetKeyDown(KeyCode.Escape))
            IsFocused = false;
        if (Input.GetMouseButtonDown(0) && !IsFocused)
            IsFocused = true;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            isSprinting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            isSprinting = false;
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            jumpRequest = true;

        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");


        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {

            if (scroll > 0)
                selectedBlockIndex++;
            else
                selectedBlockIndex--;

            if (selectedBlockIndex > (byte)(world.blocktypes.Length - 1))
                selectedBlockIndex = 1;
            if (selectedBlockIndex < 1)
                selectedBlockIndex = (byte)(world.blocktypes.Length - 1);

        }

        if (highlightBlock.gameObject.activeSelf)
        {

            // Destroy block.
            if (Input.GetMouseButtonDown(0)){

                Vector3 destroyPos = PositionHelper.PixelToHex(highlightBlock.position);
                world.EditHex(destroyPos, 0);
            }

            // Place block.
            if (Input.GetMouseButtonDown(1)){

                Vector3 placePos = PositionHelper.PixelToHex(placeBlock.position);
                world.EditHex(placePos, selectedBlockIndex);
            }
        }

    }


    private void PlaceCursorBlocks()
    {

        float step = rayIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);
            Vector3 checkPos = PositionHelper.PixelToHex(pos);

            if (world.CheckForHex(checkPos)||world.CheckForTransparentHex(checkPos))
            {

                highlightBlock.position = new Vector3(pos.x, pos.y, pos.z);
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(pos.x, pos.y, pos.z);

            step += rayIncrement;

        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }


    private void UpdateTransform()
    {
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);
    }


    private float CheckDownSpeed(float downSpeed)
    {

        if (
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth))) ||
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth))) ||
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)))||
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)))
           )
        {

            isGrounded = true;
            return 0;

        }
        else
        {

            isGrounded = false;
            return downSpeed;

        }

    }


    private float CheckUpSpeed(float upSpeed)
    {

        if (
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth))) ||
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth))) ||
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))) ||
            world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)))
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }

    }


    private bool CheckFront
    {

        get
        {
            if (
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth))) ||
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)))
                )
                return true;
            else
                return false;
        }

    }


    private bool CheckBack
    {

        get
        {
            if (
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth))) ||
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))
                )
                return true;
            else
                return false;
        }

    }


    private bool CheckLeft
    {

        get
        {
            if (
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z))) ||
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)))
                )
                return true;
            else
                return false;
        }

    }


    private bool CheckRight
    {

        get
        {
            if (
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z))) ||
                world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)))
                )
                return true;
            else
                return false;
        }

    }


}