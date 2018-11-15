# Cascade
Cascade Language of Descriptions (Cascade)

.NET, Mono and Unity3d совместимая библиотека для чтения/записи конфигурационных данных, описаний объектов и настроек (в частности игровых: NPC, Quests, GUI, Options, Game and World Settings and etc.) из/в текстовых файлов, потоков и прочего.

Она позоляет конвертировать текстовое описание сразу в C# объект и наоборот (like a Newtonsoft JSON library). Также можно получить данные в формате иерархического дерева (Keys, Values).

Текстовый формат легко понимается и читается человеком и имеет минимум синтаксического мусора.

## Текстовое описание объектов.
  Сначала мы использовали JSON, который в целом удобен. Но с увеличением количества объектов, которых необходимо было описать, их объема появилась потребность в переиспользовании большого количества одинаковых или очень похожих описаний.

  Например, есть описание в текстовом файле параметров поведения НПС, его действий, разговоров, стандартных целей. Это большой по объему и сложный по смыслу файл, который редактируется и правится не программистами, а геймдизайнерами. Есть определенное количество НПС, описание которых практически повторяет стандартное, но имеет ряд небольших отличий. Здесь и пригодится схема когда мы "наследуем" стандартное описание для каждого НПС, меняя в нем небольшое количество уникальных для НПС параметров.

Упрощенный пример: НПС, которые ходят одинаково.
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

Очевидно, что копирование параметров приведет к классическим проблемам: много текста, сложно поменять значение параметра у всех.
Можно выделить параметры движения в отдельное описание и вставить его в описание каждого НПС.

Файл MoveDescr:
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

## Как прочитать это описание в коде (С#).

Описываем структуру НПС и как он движется:
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

Создаем свой менеджер загрузки, который включает в себя сериализатор Cascade:
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

Пару слов про интерфейсы, которые он должен реализовать, что бы создать сериализатор Cascade.
- ILogPrinter - сериализатор будет выдавать с помощью этого интерфейса ошибки при считывании и парсинге текста.
- IParserOwner - в тексте описания НПС написано, что MovingParams мы хотим вычитать из файла MoverDescrs.cscdt и ключа StandartMan. Сам сериализатор не читает файлы, а просит это сделать хозяина с помощью метода: string GetTextFromFile(string inFileName, object inContextData). В нашем случае в агрументе метода inFileName будет стоять "MoverDescrs.cscdt". И мы должны считать содержимое этого файла как текст и вернуть его как результат метода. Например так как показано в примере. Про аргумент "inContextData" я напишу ниже.

Теперь прочитаем данные (описания НПС) из файла.
```c#
//using из примера выше

public class CStaticDataManager : IParserOwner, ILogPrinter
{
	//здесь все из предыдущего примера

	List<SDescrNPC> _npc_list = new List<SDescrNPC>;

	void LoadNPC()
	{
		object context_data = null; //сейчас не надо
		string text = GetTextFromFile("NPC.cscd", context_data);
		_npc_list.AddRange(_serializer.Deserialize<SDescrNPC[]>(text, this));
	}
}
```
Готово!
