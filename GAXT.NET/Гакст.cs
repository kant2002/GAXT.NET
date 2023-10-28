using System.Diagnostics;
using System.Text;

var программа = $@"12>
{{
    42+
    |
    56=
    {{
        3?
        |
        47<
        {{
            4?
            |
            7?
        }}
    }}
}}!
".AsSpan();
var переменные = new long['z' - 'a' + 1];
var стекЗначений = new Stack<long>();
var стекПеременных = new Stack<long>();
var текущийСтек = стекЗначений;
var другойСтек = стекПеременных;
var стекУсловий = new Stack<long>();
var стекЦиклов = new Stack<long>();
var списокМакросов = new List<string>();
StringBuilder? текущийМакрос = null;

ВыполнитьПрограмму(программа, Console.Out);

void ВыполнитьПрограмму(ReadOnlySpan<char> програма, TextWriter писатель)
{
    int начальнаяПозиция = 0;
    for (int i = начальнаяПозиция; i < програма.Length; i++)
    {
        var команда = програма[i];
        if (ВыполнитьКоманду(команда, i, писатель))
        {
            break;
        }
    }
}

bool НадоВыполнять() => стекУсловий.Count == 0 || стекУсловий.All(_ => _ == 1);

long Посмотреть()
{
    var з = текущийСтек.Peek();
    if (текущийСтек == стекПеременных)
    {
        return переменные[з];
    }

    return з;
}

long Снять()
{
    var b = текущийСтек.Pop();
    if (текущийСтек == стекПеременных)
    {
        return переменные[b];
    }

    return b;
}

bool ВыполнитьКоманду(char команда, int currentOperand, TextWriter писатель)
{
    if (текущийМакрос is not null)
    {
        if (команда != ')')
        {
            текущийМакрос.Append(команда);
        }
        else
        {
            списокМакросов.Add(текущийМакрос.ToString());
            Debug.WriteLine($"Added macro '{списокМакросов.Last()}'");
            текущийМакрос = null;
        }

        return false;
    }

    if (команда is >= 'A' and <= 'Z')
    {
        if (!НадоВыполнять()) return false;

        текущийСтек.Push(ПолучитьКонстантноеЗначение(команда));
    }
    else if (команда is >= 'a' and <= 'z')
    {
        if (!НадоВыполнять()) return false;

        стекПеременных.Push(команда - 'a');
    }
    else if (команда is >= '0' and <= '9')
    {
        if (!НадоВыполнять()) return false;
        текущийСтек.Push(команда - '0');
    }
    else if (команда == '+')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 + оп2;
        текущийСтек.Push(значение);
    }
    else if (команда == '-')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 - оп2;
        текущийСтек.Push(значение);
    }
    else if (команда == '*')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 * оп2;
        текущийСтек.Push(значение);
    }
    else if (команда == '/')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 / оп2;
        текущийСтек.Push(значение);
    }
    else if (команда == '<')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 < оп2 ? 1 : 0;
        текущийСтек.Push(значение);
    }
    else if (команда == '>')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 > оп2 ? 1 : 0;
        текущийСтек.Push(значение);
    }
    else if (команда == '=')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var значение = оп1 == оп2 ? 1 : 0;
        текущийСтек.Push(значение);
    }
    else if (команда == '_')
    {
        if (!НадоВыполнять()) return false;
        var оп2 = Снять();
        var оп1 = Снять();
        var sign1 = Math.Abs(оп1) == оп1 ? 1 : -1;
        var sign2 = Math.Abs(оп2) == оп2 ? 1 : -1;
        var значение = sign1 * sign2 * long.Parse(Math.Abs(оп1).ToString() + Math.Abs(оп2).ToString());
        текущийСтек.Push(значение);
    }
    else if (команда == '$')
    {
        if (!НадоВыполнять()) return false;
        var оп1 = Посмотреть();
        писатель.Write((char)оп1);
    }
    else if (команда == '?')
    {
        if (!НадоВыполнять()) return false;
        var оп1 = Посмотреть();
        писатель.Write(оп1);
    }
    else if (команда == '~')
    {
        if (!НадоВыполнять()) return false;
        _ = Снять();
    }
    else if (команда == '{')
    {
        var condition = Посмотреть();
        стекУсловий.Push(condition);
    }
    else if (команда == '|')
    {
        var condition = стекУсловий.Pop();
        стекУсловий.Push(1 - condition);
    }
    else if (команда == '}')
    {
        _ = стекУсловий.Pop();
    }
    else if (команда == '(')
    {
        текущийМакрос = new StringBuilder();
    }
    else if (команда == ')')
    {
        списокМакросов.Add(текущийМакрос.ToString());
    }
    else if (команда == '!')
    {
        if (!НадоВыполнять()) return false;
        return true;
    }
    else if (команда == ':')
    {
        if (!НадоВыполнять()) return false;
        if (текущийСтек == стекПеременных)
        {
            var оп2 = Посмотреть();
            Снять();
            другойСтек.Push(оп2);
        }
        else
        {
            var оп2 = Снять();
            var оп1 = другойСтек.Peek();
            переменные[оп1] = оп2;
            Debug.WriteLine($"Set variable {(char)(оп1 + 'a')} to значение {оп2}");
        }
    }
    else if (команда == '#')
    {
        if (!НадоВыполнять()) return false;
        (текущийСтек, другойСтек) = (другойСтек, текущийСтек);
    }
    else if (команда == '[')
    {
        if (!НадоВыполнять()) return false;
        стекЦиклов.Push(currentOperand);
    }
    else if (команда == ']')
    {
        if (!НадоВыполнять()) return false;
        _ = стекЦиклов.Pop();
    }
    else if (команда == '@')
    {
        if (!НадоВыполнять()) return false;
        var кодМакроса = Снять();
        var макрос = списокМакросов[(int)кодМакроса];
        ВыполнитьПрограмму(макрос, писатель);
    }
    else if (char.IsWhiteSpace(команда))
    {
        // ничего не делаем.
    }
    else
    {
        throw new InvalidOperationException($"Неизвестный опкод {команда}");
    }

    return false;
}

long ПолучитьКонстантноеЗначение(char константа)
{
    return константа switch
    {
        'A' => 10,
        'B' => 20,
        'C' => 30,
        'D' => 40,
        'E' => 50,
        'F' => 60,
        'G' => 70,
        'H' => 80,
        'I' => 90,
        'J' => 100,
        'K' => 200,
        'L' => 300,
        'M' => 400,
        'N' => 500,
        'O' => 600,
        'P' => 700,
        'Q' => 800,
        'R' => 900,
        'S' => 1000,
        'T' => 2000,
        'U' => 3000,
        'V' => 4000,
        'W' => 5000,
        'X' => 6000,
        'Y' => 7000,
        'Z' => 8000,
        _ => throw new InvalidOperationException()
    };
}