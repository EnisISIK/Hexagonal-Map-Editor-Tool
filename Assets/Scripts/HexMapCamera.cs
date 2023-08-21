using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
	private World world;

	public Transform highlightBlock;
	public Transform placeBlock;
	public Transform cam;

	Transform swivel, stick;
	float zoom = 1f;

	public float stickMinZoom, stickMaxZoom;
	public float swivelMinZoom, swivelMaxZoom;
	public float moveSpeedMinZoom, moveSpeedMaxZoom;
	public float rotationSpeed;

	public float checkIncrement = 0.1f;
	public float reach = 8f;

	void Awake()
	{
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
	}

	private void Start()
	{
		world = GameObject.Find("World").GetComponent<World>();
	}

	void Update()
	{
		if (highlightBlock.gameObject.activeSelf)
		{

			// Destroy block.
			if (Input.GetMouseButtonDown(1)){

				Vector3 destroyPos = world.PixelToHex(highlightBlock.position);
				world.GetChunkFromChunkVector3(destroyPos).EditHex(destroyPos, 0);  //şimdidide raycastta bir problem var onu çöz tamamdır
				Debug.Log(world.PixelToHex(highlightBlock.position));
			}
			// Place block.
			if (Input.GetMouseButtonDown(0)){
				Vector3 placePos = world.PixelToHex(placeBlock.position);
				world.GetChunkFromChunkVector3(placePos).EditHex(placePos, 1);
			}
		}

		float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
		if (zoomDelta != 0f)
		{
			AdjustZoom(zoomDelta);
		}

		float rotationDelta = Input.GetAxis("Rotation");
		if (rotationDelta != 0f)
		{
			AdjustRotation(rotationDelta);
		}

		float xDelta = Input.GetAxis("Horizontal");
		float zDelta = Input.GetAxis("Vertical");
		if (xDelta != 0f || zDelta != 0f)
		{
			AdjustPosition(xDelta, zDelta);
		}

		placeCursorBlocks();
	}

	public float moveSpeed;
	void AdjustPosition(float xDelta, float zDelta)
	{
		Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
		float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
		float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom)  * damping * Time.deltaTime;

		Vector3 position = transform.localPosition;
		position += direction * distance;
		transform.localPosition = position;
	}

	void AdjustZoom(float delta)
	{
		zoom = Mathf.Clamp01(zoom + delta);

		float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

		float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	float rotationAngle;
	void AdjustRotation(float delta)
	{
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		if (rotationAngle < 0f)
		{
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f)
		{
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	}

	private void placeCursorBlocks()
	{
		float step = checkIncrement;
		Vector3 lastPos = new Vector3();

		while (step < reach)
		{
			Vector3 pos = cam.position + (cam.forward * step);
			Vector3 checkPos = world.PixelToHex(pos);

			if (world.CheckForHex(checkPos))
			{
				highlightBlock.position = new Vector3(pos.x, pos.y, pos.z);
				placeBlock.position = lastPos;

				highlightBlock.gameObject.SetActive(true);
				placeBlock.gameObject.SetActive(true);

				return;
			}

			lastPos = new Vector3(pos.x, pos.y, pos.z);

			step += checkIncrement;
		}

		highlightBlock.gameObject.SetActive(false);
		placeBlock.gameObject.SetActive(false);
	}
}