#pragma strict

var target : Transform;
var distance : float = 10.0;

var scrollSpeed : float = 5;

var xSpeed : float = 250.0;
var ySpeed : float = 120.0;

var yMinLimit : float = -20;
var yMaxLimit : float = 80;

var distanceMin : float = 40;
var distanceMax : float = 130;

var x : float = 0.0;
var y : float = 0.0;

 
function Start ()
{
	var angles = transform.eulerAngles;
	x = angles.y;
	y = angles.x;
 
	// Make the rigid body not change rotation
	if (GetComponent.<Rigidbody>())
	{
		GetComponent.<Rigidbody>().freezeRotation = true;
	}
}
 
function LateUpdate ()
{
	if (target)
	{
		var rotation = Quaternion.Euler(y, x, 0);
		var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
	
		distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollSpeed, distanceMin, distanceMax);
	
		var hit : RaycastHit;
	
		if (Physics.Linecast (target.position, transform.position, hit))
		{
			distance -=  hit.distance;
		}
		
		transform.rotation = rotation;
		transform.position = position;
		
		if (Input.GetMouseButton(1))
		{
			Cursor.visible = false;
		
			if (y > 90 && y < 275)
			{
				x -= Input.GetAxis("Mouse X") * xSpeed * distance * 0.02;
				y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}
			else if (y < -275 && y > -90)
			{
				x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02;
				y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}
			else if (y < -90 && y > -275)
			{
				x -= Input.GetAxis("Mouse X") * xSpeed * distance * 0.02;
				y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}
			else if (y > 275 && y < 90)
			{
				x -= Input.GetAxis("Mouse X") * xSpeed * distance * 0.02;
				y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}
			else
			{
				x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02;
				y -= Input.GetAxis("Mouse Y") * ySpeed * distance * 0.02;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
			}
			
			
		}
		else
		{
			Cursor.visible = true;
		}
	}
	
	
	
}
 
 
static function ClampAngle (angle : float, min : float, max : float)
{
	if (angle < -360)
	{
		angle += 360;
	}
	if (angle > 360)
	{
		angle -= 360;
	}
	return Mathf.Clamp (angle, min, max);
}