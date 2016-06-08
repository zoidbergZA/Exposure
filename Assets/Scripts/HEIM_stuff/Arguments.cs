using UnityEngine;
using System.Collections;
using System;

public class Arguments : MonoBehaviour
{
	private int userID;
	private int gameID;
	private string username;
	private int gametime;
	private string conURL;
	
	void Awake()
    {
		/*string[] arguments = Environment.GetCommandLineArgs();
		userID = Convert.ToInt32(arguments[2]);
		gameID = Convert.ToInt32(arguments[3]);
		username = arguments[4];
		gametime = Convert.ToInt32(arguments[5]);
		conURL = arguments[6];*/
	}
	
	public int getUserID()
    {
		return userID;	
	}
	
	public int getGameID()
    {
		return gameID;	
	}
	
	public string getUsername() 
    {
		return username;
	}
	
	public int getGameTime()
    {
		return gametime;
	}

	public string getConURL()
    {
		return conURL;
	}
}
