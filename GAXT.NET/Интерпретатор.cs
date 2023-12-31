﻿namespace GAXT.NET;
using static GAXT.NET.СредаВыполнения;

internal static class Интерпретатор
{
    public static void Выполнить(string программа)
    {
        var лексер = new ЛексерГакст(программа);
        var парсер = new ПарсерГакст(лексер);
        var результатРазбора = парсер.ParseПрограмма();
        if (результатРазбора.IsError)
        {
            Console.WriteLine($"ошибка разбора программы. Получили {результатРазбора.Error.Got} на строке и столбце {результатРазбора.Error.Position}");
            return;
        }

        var областьВыполнения = new ОбластьВыполнения(null);
        ВыполнитьБлок(областьВыполнения, результатРазбора.Ok.Value);
    }

    static void ВыполнитьБлок(ОбластьВыполнения областьВыполнения, Блок блокКода)
    {
        foreach (var выражение in блокКода.Выражения)
        {
            if (областьВыполнения.Прекратить) break;
            ВыполнитьВыражение(областьВыполнения, выражение);
        }
    }

    static void ВыполнитьВыражение(ОбластьВыполнения областьВыполнения, Выражение выражение)
    {
        //Console.WriteLine($"Перед выполнением {выражение}");
        switch (выражение)
        {
            case ПростойКод блокКода:
                for (int i = 0; i < блокКода.Код.Length; i++)
                {
                    if (областьВыполнения.Прекратить) break;
                    char команда = блокКода.Код[i];
                    ВыполнитьКоманду(областьВыполнения, команда);
                }
                break;
            case Блок блокКода:
                ВыполнитьБлок(областьВыполнения, блокКода);
                break;
            case УсловноеВыражение условие:
                var условиеВыбора = Посмотреть();
                if (условиеВыбора != 0)
                {
                    ВыполнитьВыражение(областьВыполнения, условие.ИстинноеВыражение);
                }
                else
                {
                    ВыполнитьВыражение(областьВыполнения, условие.ЛожноеВыражение);
                }
                break;
            case Макрос блокКода:
                ЗарегистрироватьМакрос(() =>
                {
                    var дочерняяОбластьВыполнения = new ОбластьВыполнения(областьВыполнения);
                    ВыполнитьВыражение(дочерняяОбластьВыполнения, блокКода.Тело);
                });
                break;
            case ЗацикленноеВыражение блокКода:
                {
                    var дочерняяОбластьВыполнения = new ОбластьВыполнения(областьВыполнения);
                    do
                    {
                        ВыполнитьВыражение(дочерняяОбластьВыполнения, блокКода.Тело);
                        if (дочерняяОбластьВыполнения.Прекратить) break;
                        var условиеВыходаИзЦикла = ТранслироватьПеременную(Посмотреть());
                        if (условиеВыходаИзЦикла == 0) break;
                    }
                    while (true);
                }
                break;
            default:
                throw new InvalidOperationException("Неизвестное выражение");
        };
        //Console.WriteLine($"После выполнения {выражение}");
        //DumpInterpreterState();
        //Console.WriteLine();
    }

    static void ВыполнитьКоманду(ОбластьВыполнения областьВыполнения, char команда)
    {
        if (команда is >= 'A' and <= 'Z')
        {
            var значение = ПолучитьКонстантноеЗначение(команда);
            ПоложитьВСтек(значение);
        }
        else if (команда is >= 'a' and <= 'z')
        {
            ПоложитьПеременную(команда);
        }
        else if (команда is >= '0' and <= '9')
        {
            var значение = команда - '0';
            ПоложитьВСтек(значение);
        }
        else if (команда == '+')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) + ТранслироватьПеременную(оп2);
            СохранитьПеременную(значение);
        }
        else if (команда == '-')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) - ТранслироватьПеременную(оп2);
            СохранитьПеременную(значение);
        }
        else if (команда == '*')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) * ТранслироватьПеременную(оп2);
            СохранитьПеременную(значение);
        }
        else if (команда == '/')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) / ТранслироватьПеременную(оп2);
            СохранитьПеременную(значение);
        }
        else if (команда == '<')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) < ТранслироватьПеременную(оп2) ? 1 : 0;
            СохранитьПеременную(значение);
        }
        else if (команда == '>')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) > ТранслироватьПеременную(оп2) ? 1 : 0;
            СохранитьПеременную(значение);
        }
        else if (команда == '=')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var значение = ТранслироватьПеременную(оп1) == ТранслироватьПеременную(оп2) ? 1 : 0;
            СохранитьПеременную(значение);
        }
        else if (команда == '_')
        {
            var оп2 = СнятьСоСтека();
            var оп1 = Посмотреть();
            var топ1 = ТранслироватьПеременную(оп1);
            var топ2 = ТранслироватьПеременную(оп2);
            var значение = СклеитьЗначения(топ1, топ2);
            СохранитьПеременную(значение);
        }
        else if (команда == '$')
        {
            var оп1 = Посмотреть();
            ВывестиСимвол(ТранслироватьПеременную(оп1));
        }
        else if (команда == '?')
        {
            var оп1 = Посмотреть();
            ВывестиЧисло(ТранслироватьПеременную(оп1));
        }
        else if (команда == '~')
        {
            _ = СнятьСоСтека();
        }
        else if (команда == '%')
        {
            ОчиститьСтек();
        }
        else if (команда == ':')
        {
            var значение = СнятьСоСтека();
            ПереключитьТекущийСтек();
            СохранитьПеременную(значение);
            ПереключитьТекущийСтек();
        }
        else if (команда == '#')
        {
            ПереключитьТекущийСтек();
        }
        else if (команда == '@')
        {
            var кодМакроса = СнятьСоСтека();
            var макрос = ПолучитьМакрос(кодМакроса);
            макрос();
        }
        else if (команда == '\\')
        {
            областьВыполнения.Прервать();
        }
        else if (char.IsWhiteSpace(команда))
        {
            // ничего не делаем.
        }
        else
        {
            // ничего не делаем.
        }
    }

    static long ПолучитьКонстантноеЗначение(char константа)
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

    class ОбластьВыполнения
    {
        private readonly ОбластьВыполнения? родительскаяОбластьВыполнения;

        public ОбластьВыполнения(ОбластьВыполнения? родительскаяОбластьВыполнения)
        {
            this.родительскаяОбластьВыполнения = родительскаяОбластьВыполнения;
        }

        public bool Прекратить { get; private set; }

        public void Прервать()
        {
            Прекратить = true;
        }
    }
}
