using UnityEngine;


public class FlyCam : MonoBehaviour
{
	/*
	/	A first person Free Fly Camera controller capable of destroying 
	/	and placing blocks. Blocks dont have colliders, because of that 
	/	fly camera controller uses methods for checking block data.
	*/

	[SerializeField]
	private float acceleration = 50;
	[SerializeField]
	private float sprintSpeed = 4;
	[SerializeField]
	private float lookSensitivity = 1;
	[SerializeField]
	private float damping = 5;
	[SerializeField]
	private float rayIncrement = 0.1f;
	[SerializeField]
	private float reach = 8f;
	[SerializeField]
	private bool focusOnEnable = true;

	[SerializeField]
	private float playerWidth = 0.15f;

	[SerializeField]
	private Transform highlightBlock;
	[SerializeField]
	private Transform placeBlock;
	[SerializeField]
	private World world;

	private Vector3 velocity = new Vector3(0,0,0);
	private byte selectedBlockIndex = 1;
	private float mouseHorizontal;
	private float mouseVertical;
	private Vector3 moveVector = new Vector3(0f, 0f, 0f);

	private bool IsSprinting = false;

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
		world = GameObject.Find("World").GetComponent<World>();
	}


    private void Update()
	{
		GetPlayerInputs();
		PlaceCursorBlocks();

		if (IsFocused)
			UpdateTransform();
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
		if (Input.GetKey(KeyCode.Space))
			moveVector += Vector3.up;
		if (Input.GetKey(KeyCode.LeftControl))
			moveVector += Vector3.down;
		if (Input.GetKeyDown(KeyCode.Escape))
			IsFocused = false;
		if (Input.GetMouseButtonDown(0) && !IsFocused)
			IsFocused = true;

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
			if (Input.GetMouseButtonDown(0))
			{
				Vector3 destroyPos = PositionHelper.PixelToHex(highlightBlock.position);
				world.EditHex(destroyPos, 0);
			}

			// Place block.
			if (Input.GetMouseButtonDown(1))
			{
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

			Vector3 pos = transform.position + (transform.forward * step);
			Vector3 checkPos = PositionHelper.PixelToHex(pos);

			if (world.CheckForHex(checkPos) || world.CheckForTransparentHex(checkPos))
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
		// Velocity
		velocity += GetAcceleration() * Time.deltaTime;

		if ((velocity.z > 0 && CheckFront) || (velocity.z < 0 && CheckBack))
			velocity.z = 0;
		if ((velocity.x > 0 && CheckRight) || (velocity.x < 0 && CheckLeft))
			velocity.x = 0;
		if (velocity.y < 0 && CheckDown())
			velocity.y = 0;

		velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.deltaTime);
		transform.position += velocity * Time.deltaTime;

		moveVector = new Vector3(0f, 0f, 0f);

		// Rotation
		Vector2 mouseDelta = lookSensitivity * new Vector2(mouseHorizontal, - mouseVertical);
		Quaternion rotation = transform.rotation;
		Quaternion horizontalAngle = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
		Quaternion verticalAngle = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
		transform.rotation = horizontalAngle * rotation * verticalAngle;

	}


	private Vector3 GetAcceleration()
	{
		Vector3 direction = transform.TransformVector(moveVector.normalized);

		if (Input.GetKey(KeyCode.LeftShift))
			return direction * (acceleration * sprintSpeed); 
		return direction * acceleration;
	}


	private bool CheckDown()
	{

		if (
			world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y - 1, transform.position.z - playerWidth))) ||
			world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y - 1, transform.position.z - playerWidth))) ||
			world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x + playerWidth, transform.position.y - 1, transform.position.z + playerWidth))) ||
			world.CheckForHex(PositionHelper.PixelToHex(new Vector3(transform.position.x - playerWidth, transform.position.y - 1, transform.position.z + playerWidth)))
		   )
			return true;
		else
			return false;

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