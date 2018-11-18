# Cascade
Cascade Language of Descriptions (Cascade)

Cascade is compatible with .Net, Mono and Unity3d library for writing/reading to/from text files, streams and other configuration data, object descriptions and settings.

It allows to convert text description to C# object and vice versa (like a Newtonsoft JSON Library). Also, it is possible to get data in hierarchical tree format (Keys, Values).

Text format is easy to read and understand and has minimum of syntactic garbage.

## Text object description.

At the beginning JSON was used, which is convenient in general. However, with increasing number of objects and their complexity it was necessary to reuse a lot of the same or almost the same descriptions.

For instance, there is description in text file of NPC behaviour parameters like its actions, dialogues, default goals. This is large and complex file, which is edited and changed not by programmers, but by game designers.
There is certain number of NPC with almost the same description as the default one, but with minor differences. In such cases default description "inheritance" for each NPC with unique parameters changes can be applicable.


Simple example: NPCs with the movement behaviour.

```c#
Name: Swordman
MovingParams:
	Walk: 10
	Run: 15
	AngleSpeed: 25
Weapon: Sword
--
Name: Axeman
MovingParams:
	Walk: 10
	Run: 15
	AngleSpeed: 25  
Weapon: Axe
--
Name: Spearman
MovingParams:
	Walk: 10
	Run: 15
	AngleSpeed: 25  
Weapon: Spear
```

This is obvious that copying of parameters will lead to well-known problems: a lot of text, difficult to change parameter value for each NPC.

However, it is possible to highlight the movement parameters 
in a separate description and insert it into the description of each NPC.

File MoveDescr:
```c#
Walkman:
	Walk: 10
	Run: 15
	AngleSpeed: 25
```

Файл NPCDescr:
```c#
Name: Swordman
MovingParams:
	#insert file:MoverDescrs.cscdt key:StandartMan
Weapon: Sword
--
Name: Axeman
MovingParams:
	#insert file:MoverDescrs.cscdt key:StandartMan
Weapon: Axe
--
Name: Spearman
MovingParams:
	#insert file:MoverDescrs.cscdt key:StandartMan
Weapon: Spear
```

## How to read this description in code(C#).

Describe the structure of NPc and its movement:
```c#
struct SDescrMove
{
  public float Walk { get; private set; }
  public float Run { get; private set; }
  public float AngleSpeed { get; private set; }
}

struct SDescrNPC
{
  public string Name { get; private set; }
  public SDescrMove MovingParams { get; private set; }
  public string Weapon { get; private set; }
}
```

Create our own upload manager, which includes the Cascade serializer:
```c#
using System;
using CascadeParser;
using CascadeSerializer;
using System.Collections.Generic;

public class CStaticDataManager : IParserOwner, ILogPrinter
{
	protected CCascadeSerializer _serializer;

	public CStaticDataManager()
	{
		_serializer = new CCascadeSerializer(this);
	}

	//IParserOwner requests this method
	public string GetTextFromFile(string inFileName, object inContextData)
	{
		string path = Path.Combine(Application.StartupPath, "Data", inFileName);
		if (!File.Exists(path))
			return string.Empty;
		return File.ReadAllText(path);
	}

	#region ILogPrinter
	public void LogError(string inText) { Console.WriteLine("Error: {0}", inText); }
	public void LogWarning(string inText) { Console.WriteLine("Warning: {0}", inText); }
	public void Trace(string inText) { Console.WriteLine(inText); }
	#endregion ILogPrinter
}
```

A few words about the interfaces that it needs to implement in order to create the Cascade serializer.
- ILogPrinter - serializer will show errors using this interface when reading and parsing text.
- IParserOwner - description of the NPC says that we want to read MovingParams from MoverDescrs.cscdt file and the StandartMan key.

The serializer itself does not read the files, but asks the host to do this using the method: string GetTextFromFile (string inFileName, object inContextData).
In our case, in the argument of the inFileName method will be "MoverDescrs.cscdt".
And we must read the content of this file as text and return it as the result of a method. 
As shown in the example below. About the argument "inContextData" I will write further

Now read the data (description of the NPC) from the file.
```c#
//using из примера выше

public class CStaticDataManager : IParserOwner, ILogPrinter
{
	//здесь все из предыдущего примера

	List<SDescrNPC> _npc_list = new List<SDescrNPC>;

	void LoadNPC()
	{
		//здесь можно создать объект, который будет передаваться в метод
		//string GetTextFromFile(string inFileName, object inContextData)
		//как inContextData
		object context_data = null; 
		
		string text = GetTextFromFile("NPC.cscd", context_data);
		_npc_list.AddRange(_serializer.Deserialize<SDescrNPC[]>(text, this));
	}
}
```
Done!
