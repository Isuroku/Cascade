# Cascade
Cascade Language of Descriptions (Cascade)

  **Цель этого проекта** - разработать язык для описания объектов (в частности игровых: NPC, Quests, GUI, Options, Game and World Settings and etc.), который легко бы понимался и писался человеком в текстовых файлах. А также парсер на C# для использования его в Unity3D.

  Сейчас я использую JSON, который в целом удобен, но мне нужена возможность включать в описание другое описание, которое может повторяться от объекта к объекту.

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

## Общее описание
Представляет собой иерархическое дерево в форме: **ключ = список значений + список "дочерних" ключей**.
```sh
Ключ: строковый литерал, имя, которое уникально среди других ключей его родителя.
Значение: простой объект (строка, число) или составной объект (набор ключей- значений).
```
Ключ отделен от его значений двоеточием. Разделителем в списке значений является пробел (запятая - дополнительный символ для повышения читаемости).

Иерархичность (вложенность) передается новой строкой и табуляторным смещением подобно языку Python. Вложенность имеет ключевое значение при определении структуры написанного.


**Простые примеры:**
```sh
Key: Value //Classic Key - Value

Key1: 1, 2, 3 //Key - List of Numbers (simple objects)

Key2: //Key - List of child keys
    Key21: value1, value2
    Key22: value3, value4 //Key - Values (simple objects) + Child Keys
        Key223: value6, value6

Key3:
    //First complex object
    Key1: 234
    Key2: some_string
    -- //special divider
    //Second complex object
    Key1: 987
    Key2: other_string
```
**Строковое значение:**
это последовательность символов Unicode, которые либо заключены в двойные кавычки, либо являются непрерывной последовательностью символов букв, цифр или знака подчеркивания "_".  Часто в описаниях параметр передается словом - строкой без разделений пробелом и без специальных символов - для них и сделана упрощенная форма строки без кавычек.
```sh
Key: string_value1, "This string value 2 has symbols: ^&$@ _ !"
```
