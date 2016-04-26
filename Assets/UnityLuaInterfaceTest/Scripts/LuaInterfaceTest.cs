using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using LuaInterface;

public class LuaInterfaceTest : MonoBehaviour
{
	public ExamplePlayer _player;

	Lua lua = new Lua();

	string output = "";

	void Start()
	{
		GetComponent<GUIText>().text = "";

		output += "Global Variables\n";
		output += "--------------------------------------------------------\n";
		lua.DoString("num = 2"); // creates a Lua global variable called num and assign 2 to it
		double num = (double)lua["num"];
		num = lua.GetNumber("num"); // convenience function that does the same thing
		output += "num: " + num + "\n";

		lua["str"] = "a string"; // create a Lua global variable str and assign a string to it.
		string str = (string)lua["str"];
		str = lua.GetString("str"); // again, a convenience function that does the same thing
		output += "str: " + str + "\n";
		
		lua.DoString("str = 'another string'"); // write to Lua global variable str. strings can be in single-quotes or double quotes
		str = lua.GetString("str"); // retrieve the updated value
		output += "str: " + str + "\n";

		lua["num"] = 32+32;
		num = lua.GetNumber("num");
		output += "num: " + num + "\n";

		lua["luabool"] = true;
		bool gotBool = lua.GetBoolean("luabool");
		output += "gotBool: " + gotBool + "\n";

		output += "\n\n";



		output += "Using Tables\n";
		output += "--------------------------------------------------------\n";

		lua.NewTable("table"); // create a Lua global table variable. in Lua, tables are the main data structures, acting as lists or dictionaries
		lua.DoString("table[0] = 'first element'");
		lua.DoString("table[1.50] = 2");
		lua.DoString("table[" + (1+3) + "] = true");
		lua.DoString("table[false] = 'lulz so randum'");
		LuaTable myLuaTable = lua.GetTable("table");

		// convert it to a ListDictionary
		ListDictionary tableDict = lua.GetTableDict(myLuaTable);
		foreach (DictionaryEntry de in tableDict)
		{
			output += "table[" + de.Key + "]: " + de.Value + "\n";
		}
		output += "\n";



		// make sure this path exists. if you imported the Unity Lua Interface to a different path, edit it if need be
		string luaScriptsFolder = Application.dataPath + "/UnityLuaInterfaceTest/Scripts/LuaScripts/";

		if (System.IO.Directory.Exists(luaScriptsFolder) == false)
		{
			output += "\"" + luaScriptsFolder + "\" not found.\nEdit LuaInterfaceTest.cs and change the variable luaScriptsFolder in LuaInterfaceTest.Start()\n";
			return;
		}



		string scriptName = luaScriptsFolder + "TableTest.lua";
		output += "Going to read \"" + scriptName + "\"...\n";
		lua.DoFile(scriptName);

		myLuaTable = lua.GetTable("testTable"); // testTable comes from the script that was executed

		// treat myLuaTable as if it was a ListDictionary
		foreach (DictionaryEntry entry in myLuaTable)
		{
			output += "testTable[" + entry.Key + "]: " + entry.Value + "\n";
		}
		output += "\n\n";


		output += "Calling A Lua Function From Unity\n";
		output += "--------------------------------------------------------\n";
		scriptName = luaScriptsFolder + "Fib.lua";
		output += "Going to read \"" + scriptName + "\"...\n";
		lua.DoFile(scriptName);

		LuaFunction fastfib = lua.GetFunction("fastfib");
		object[] returnValues; // remember, Lua functions can return multiple values in a single call

		num = 14;

		output += "Going to call fastfib(n) in a loop " + num + " times...\n";
		output += "Using DoString(): ";
		for (int n = 0; n < num; ++n)
		{
			returnValues = lua.DoString("return fastfib(" + n + ")");
			output += returnValues[0] + ((n == num-1) ? "" : ", ");
		}
		output += "\n";
		output += "Using LuaFunction.Call(): ";
		for (int n = 0; n < num; ++n)
		{
			returnValues = fastfib.Call(n);
			output += returnValues[0] + ((n == num-1) ? "" : ", ");
		}
		output += "\n\n\n";


		output += "Calling A Unity Function From Lua\n";
		output += "--------------------------------------------------------\n";
		// Create a Lua global function called SaySomething and "link" this LuaInterfaceTest.ActuallySaySomething to it
		lua.RegisterFunction("SaySomething", this, this.GetType().GetMethod("ActuallySaySomething"));

		scriptName = luaScriptsFolder + "CallUnityFunctionFromLua.lua";
		output += "Going to read \"" + scriptName + "\"...\n";
		lua.DoFile(scriptName);
		output += "\n\n";


		output += "Passing A Unity Script Component To Lua\n";
		output += "--------------------------------------------------------\n";
		scriptName = luaScriptsFolder + "PassUnityObjectToLua.lua";
		output += "Going to read \"" + scriptName + "\"...\n";
		lua.DoFile(scriptName);

		LuaFunction doSomethingFromLua = lua.GetFunction("DoSomethingFromLua");
		output += "Going to call DoSomethingFromLua(ExamplePlayer)...\n";
		doSomethingFromLua.Call(_player);
		output += "\n\n";


		output += "Catching Errors In Lua Scripts\n";
		output += "--------------------------------------------------------\n";
		scriptName = luaScriptsFolder + "SyntaxError.lua";
		output += "Going to read \"" + scriptName + "\"...\n";
		output += "Expect a LuaException error here...\n";
		try
		{
			lua.DoFile(scriptName);
		}
		catch (LuaException e)
		{
			// fail gracefully
			output += e + "\n";
		}
		output += "\n\n";
	}

	// the function has to be public for Lua to be able to call it
	public void ActuallySaySomething(int numberOfTimes, string something)
	{
		output += "Called ActuallySaySomething() with parameters: " + numberOfTimes + ", \"" + something + "\"\n";
	}

	Vector2 scrollPosition = Vector2.zero;
	void OnGUI()
	{
		//GUILayout.BeginHorizontal();

		//GUILayout.Label("", GUILayout.Width(390));

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.Label(output);
		GUILayout.EndScrollView();

		//GUILayout.EndHorizontal();
	}
}

