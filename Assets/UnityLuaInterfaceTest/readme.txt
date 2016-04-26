Unity Lua Interface Library

by Anomalous Underdog


This scripting package allows you to run and control a Lua interpreter inside Unity.

Practical applications for this include modability features for your games, allowing players to mod your game to the extent that you want it to.

This is a very bare-bones package meant only to help you get started.




Notes

To make the example scene work, copy lua51.dll and luanet.dll to where the Unity binary file resides (in Windows, where the Unity.exe is in, most likely in "C:\Program Files\Unity\Editor\")

For release, also copy the same dll files to where your binary file (.exe file in Windows) resides.




Directions

Instantiate a Lua Interpreter by creating a new LuaInterface.Lua variable. You can use "import LuaInterface" (in UnityScript) or "using LuaInterface" (in C#) to shorten your code.

LuaInterface.Lua lua = new LuaInterface.Lua();


You can run code by feeding a string to it using LuaInterface.Lua.DoString()

lua.DoString("num = 2"); // creates a lua global variable called num and assign 2 to it


You can then get the value of a Lua variable into Unity by the [] operator:

double num = (double)lua["num"]; // (cast it to double)


Or you can just use one of the convenience functions to shorten that for you:

double num = lua.GetNumber("num");

There's also Lua.GetString(), Lua.GetTable(), Lua.GetFunction(), and I took the liberty of adding in a Lua.GetBoolean() seeing as it was not there yet.


If you have a Lua script file, you can use LuaInterface.Lua.DoFile() to execute it.

lua.DoFile(Application.dataPath + "/MyLuaScript.lua");

More example code usage can be found in LuaInterfaceTest.Start() (in file LuaInterfaceTest.cs), or read the LuaInterface User Guide pdf found in this asset package.




Licenses

This package makes use of LuaInterface 1.5.3 (http://luaforge.net/projects/luainterface/).

LuaInterface is licensed under the terms of the MIT license reproduced below.
This means that LuaInterface is free software and can be used for both academic and
commercial purposes at absolutely no cost.

===============================================================================

Copyright (C) 2003-2005 Fabio Mascarenhas de Queiroz.

Permission is hereby granted, free of charge, to any person obtaining a copy
of LuaInterface and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

===============================================================================


This package makes of Lua 5.1 (http://www.lua.org/).

Lua is free software: it may be used for any purpose, including commercial purposes, at absolutely no cost. Lua is distributed under the terms of the MIT license reproduced below.

===============================================================================

Copyright © 1994–2010 Lua.org, PUC-Rio.

Permission is hereby granted, free of charge, to any person obtaining a copy of Lua and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

===============================================================================



Licensing Note:

If you use this package in your game, the only requirement is that you give credit to the Lua and LuaInterface authors by including their copyright notice somewhere in your game (like in the credits).
