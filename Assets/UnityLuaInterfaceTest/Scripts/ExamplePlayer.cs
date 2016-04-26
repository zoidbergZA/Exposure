using UnityEngine;
using System.Collections;

public class ExamplePlayer : MonoBehaviour
{
	[SerializeField]
	int _health = 100;

	[SerializeField]
	string _playerName = "Anon";

	[SerializeField]
	float _speed = 1.0f;


	float _t = 1.0f;
	Vector3 _originalPos;
	Vector3 _targetPos;

	public int GetHealth()
	{
		Debug.Log(name + ".GetHealth() called.");
		return _health;
	}
	public void SetHealth(int newHealth)
	{
		Debug.Log(name + ".SetHealth(" + newHealth + ") called.");
		_health = newHealth;
	}

	public void SetSpeed(float newSpeed)
	{
		Debug.Log(name + ".SetSpeed(" + newSpeed + ") called.");
		_speed = newSpeed;
	}

	public void MoveTo(float x, float y, float z)
	{
		Debug.Log(name + ".MoveTo(" + x + ", " + y + ", " + z + ") called.");
		_originalPos = transform.position;
		_targetPos = new Vector3(x, y, z);
		_t = 0.0f;
	}

	void Update()
	{
		if (_t < 1.0f)
		{
			_t += Time.deltaTime * Time.timeScale * _speed;
			transform.position = Vector3.Lerp(_originalPos, _targetPos, _t);
		}
	}
}
