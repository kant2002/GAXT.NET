using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace GAXT.NET;

internal class Компилятор
{
    AssemblyDefinition сборка;
    AssemblyDefinition рантаймСборка;
    Collection<Instruction> инструции;
    MethodDefinition точкаВхода;
    private readonly string имя;

    public static void Скомпилировать(string имя, string программа)
    {
        var к = new Компилятор(имя);
        к.Скомпилировать(программа);
    }

    public Компилятор(string имя)
    {
        this.имя = имя;
        var имяСборки = new AssemblyNameDefinition(имя, new Version());
        сборка = AssemblyDefinition.CreateAssembly(имяСборки, "Primary", ModuleKind.Console);
        рантаймСборка = AssemblyDefinition.ReadAssembly("GAXT.Runtime.dll");
        var модуль = сборка.MainModule;
        var глобальныйТип = модуль.GetType("<Module>");
        точкаВхода = new MethodDefinition(
            "Вход",
            MethodAttributes.Public | MethodAttributes.Static,
            модуль.TypeSystem.Int32);
        глобальныйТип.Methods.Add(точкаВхода);
        сборка.EntryPoint = точкаВхода;
        точкаВхода.Body.Variables.Add(new VariableDefinition(модуль.TypeSystem.Int64));
        точкаВхода.Body.Variables.Add(new VariableDefinition(модуль.TypeSystem.Int64));
        инструции = точкаВхода.Body.Instructions;
    }

    public void Скомпилировать(string программа)
    {
        var лексер = new ЛексерГакст(программа);
        var парсер = new ПарсерГакст(лексер);
        var результатРазбора = парсер.ParseПрограмма();
        if (результатРазбора.IsError)
        {
            Console.WriteLine($"ошибка разбора программы. Получили {результатРазбора.Error.Got} на строке и столбце {результатРазбора.Error.Position}");
            return;
        }

        СкомпилироватьБлок(результатРазбора.Ok.Value);
        инструции.Add(Instruction.Create(OpCodes.Ldc_I4_0));
        инструции.Add(Instruction.Create(OpCodes.Ret));
        сборка.Write(имя + ".exe");
    }

    void СкомпилироватьБлок(Блок блокКода)
    {
        foreach (var выражение in блокКода.Выражения)
        {
            СкомпилироватьВыражение(выражение);
        }
    }

    void СкомпилироватьВыражение(Выражение выражение)
    {
        switch (выражение)
        {
            case ПростойКод блокКода:
                foreach (char команда in блокКода.Код)
                {
                    СкомпилироватьКоманду(команда);
                }
                break;
            case Блок блокКода:
                СкомпилироватьБлок(блокКода);
                break;
            case УсловноеВыражение условие:
                {
                    var Посмотреть = GetRuntimeHelperMethod("Посмотреть");
                    инструции.Add(Instruction.Create(OpCodes.Call, Посмотреть));
                    var обработчикМетода = точкаВхода.Body.GetILProcessor();
                    var меткаЛожнойВетки = обработчикМетода.Create(OpCodes.Nop);
                    обработчикМетода.Emit(OpCodes.Brfalse, меткаЛожнойВетки);
                    СкомпилироватьВыражение(условие.ИстинноеВыражение);
                    var меткаОкончания = обработчикМетода.Create(OpCodes.Nop);
                    обработчикМетода.Emit(OpCodes.Br, меткаОкончания);
                    обработчикМетода.Append(меткаЛожнойВетки);
                    СкомпилироватьВыражение(условие.ЛожноеВыражение);
                    обработчикМетода.Append(меткаОкончания);
                }
                break;
            //case Макрос блокКода:
            //    ЗарегистрироватьМакрос(() => ВыполнитьВыражение(блокКода.Тело));
            //    break;
            case ЗацикленноеВыражение блокКода:
                {
                    var обработчикМетода = точкаВхода.Body.GetILProcessor();
                    var началоЦикла = обработчикМетода.Create(OpCodes.Nop);
                    обработчикМетода.Append(началоЦикла);
                    СкомпилироватьВыражение(блокКода.Тело);

                    var Посмотреть = GetRuntimeHelperMethod("Посмотреть");
                    инструции.Add(Instruction.Create(OpCodes.Call, Посмотреть));
                    var ТранслироватьПеременную = GetRuntimeHelperMethod("ТранслироватьПеременную");
                    инструции.Add(Instruction.Create(OpCodes.Call, ТранслироватьПеременную));

                    обработчикМетода.Emit(OpCodes.Brtrue, началоЦикла);
                }
                break;
            default:
                throw new InvalidOperationException("Неизвестное выражение");
        };
    }

    public TypeDefinition GetRuntimeHelperType()
    {
        var runtimeHelpersType = рантаймСборка.Modules[0].GetType("GAXT.NET.СредаВыполнения");
        return runtimeHelpersType ?? throw new InvalidOperationException("Тип GAXT.NET.СредаВыполнения не был найден с рантайм сборке.");
    }

    public MethodReference GetRuntimeHelperMethod(string helperMethod)
    {
        var runtimeHelpersType = GetRuntimeHelperType();
        var method = FindMethod(runtimeHelpersType, helperMethod);
        if (method == null)
        {
            throw new InvalidOperationException($"RuntimeHelper {helperMethod} cannot be found.");
        }

        return сборка.MainModule.ImportReference(method);
    }

    public static MethodDefinition FindMethod(TypeDefinition typeDefinition, string methodName)
    {
        return typeDefinition.Methods.SingleOrDefault(method => method.Name == methodName)
               ?? throw new InvalidOperationException($"Cannot find method {methodName} on type {typeDefinition.FullName}");
    }

    void СкомпилироватьКоманду(char команда)
    {
        if (команда is >= 'A' and <= 'Z')
        {
            инструции.Add(Instruction.Create(OpCodes.Ldc_I8, ПолучитьКонстантноеЗначение(команда)));
            var ПоложитьВСтек = GetRuntimeHelperMethod("ПоложитьВСтек");
            инструции.Add(Instruction.Create(OpCodes.Call, ПоложитьВСтек));
        }
        else if (команда is >= 'a' and <= 'z')
        {
            //ПоложитьПеременную(команда);
            инструции.Add(Instruction.Create(OpCodes.Ldc_I8, (long)команда));
            var ПоложитьПеременную = GetRuntimeHelperMethod("ПоложитьПеременную");
            инструции.Add(Instruction.Create(OpCodes.Call, ПоложитьПеременную));
        }
        else if (команда is >= '0' and <= '9')
        {
            инструции.Add(Instruction.Create(OpCodes.Ldc_I8, (long)(команда - '0')));
            var ПоложитьВСтек = GetRuntimeHelperMethod("ПоложитьВСтек");
            инструции.Add(Instruction.Create(OpCodes.Call, ПоложитьВСтек));
            //инструции.Add(Instruction.Create(OpCodes.Call, GetRuntimeHelperMethod("DumpInterpreterState")));
        }
        else if (команда == '+' || команда == '-' || команда == '*' || команда == '/' || команда == '_')
        {
            var ТранслироватьПеременную = GetRuntimeHelperMethod("ТранслироватьПеременную");
            var СнятьСоСтека = GetRuntimeHelperMethod("СнятьСоСтека");
            инструции.Add(Instruction.Create(OpCodes.Call, СнятьСоСтека));
            инструции.Add(Instruction.Create(OpCodes.Call, ТранслироватьПеременную));
            var Посмотреть = GetRuntimeHelperMethod("Посмотреть");
            инструции.Add(Instruction.Create(OpCodes.Call, Посмотреть));
            инструции.Add(Instruction.Create(OpCodes.Call, ТранслироватьПеременную));
            инструции.Add(Instruction.Create(OpCodes.Stloc_0));
            инструции.Add(Instruction.Create(OpCodes.Stloc_1));
            инструции.Add(Instruction.Create(OpCodes.Ldloc_0));
            инструции.Add(Instruction.Create(OpCodes.Ldloc_1));
            switch (команда)
            {
                case '+':
                    инструции.Add(Instruction.Create(OpCodes.Add));
                    break;
                case '-':
                    инструции.Add(Instruction.Create(OpCodes.Sub));
                    break;
                case '*':
                    инструции.Add(Instruction.Create(OpCodes.Mul));
                    break;
                case '/':
                    инструции.Add(Instruction.Create(OpCodes.Div));
                    break;
                case '_':
                    var СклеитьЗначения = GetRuntimeHelperMethod("СклеитьЗначения");
                    инструции.Add(Instruction.Create(OpCodes.Call, СклеитьЗначения));
                    break;
            }
            var СохранитьПеременную = GetRuntimeHelperMethod("СохранитьПеременную");
            инструции.Add(Instruction.Create(OpCodes.Call, СохранитьПеременную));
        }
        else if (команда == '<' || команда == '>' || команда == '=')
        {
            var ТранслироватьПеременную = GetRuntimeHelperMethod("ТранслироватьПеременную");
            var СнятьСоСтека = GetRuntimeHelperMethod("СнятьСоСтека");
            инструции.Add(Instruction.Create(OpCodes.Call, СнятьСоСтека));
            инструции.Add(Instruction.Create(OpCodes.Call, ТранслироватьПеременную));
            var Посмотреть = GetRuntimeHelperMethod("Посмотреть");
            инструции.Add(Instruction.Create(OpCodes.Call, Посмотреть));
            инструции.Add(Instruction.Create(OpCodes.Call, ТранслироватьПеременную));
            инструции.Add(Instruction.Create(OpCodes.Stloc_0));
            инструции.Add(Instruction.Create(OpCodes.Stloc_1));
            инструции.Add(Instruction.Create(OpCodes.Ldloc_0));
            инструции.Add(Instruction.Create(OpCodes.Ldloc_1));
            switch (команда)
            {
                case '<':
                    инструции.Add(Instruction.Create(OpCodes.Clt));
                    break;
                case '>':
                    инструции.Add(Instruction.Create(OpCodes.Cgt));
                    break;
                case '=':
                    инструции.Add(Instruction.Create(OpCodes.Ceq));
                    break;
            }

            var обработчикМетода = точкаВхода.Body.GetILProcessor();
            var меткаЛожнойВетки = обработчикМетода.Create(OpCodes.Nop);
            обработчикМетода.Emit(OpCodes.Brfalse, меткаЛожнойВетки);
            обработчикМетода.Emit(OpCodes.Ldc_I4_1);
            var меткаОкончания = обработчикМетода.Create(OpCodes.Nop);
            обработчикМетода.Emit(OpCodes.Br, меткаОкончания);
            обработчикМетода.Append(меткаЛожнойВетки);
            обработчикМетода.Emit(OpCodes.Ldc_I4_0);
            обработчикМетода.Append(меткаОкончания);
            var СохранитьПеременную = GetRuntimeHelperMethod("СохранитьПеременную");
            инструции.Add(Instruction.Create(OpCodes.Call, СохранитьПеременную));
        }
        else if (команда == '$' || команда == '?')
        {
            var ТранслироватьПеременную = GetRuntimeHelperMethod("ТранслироватьПеременную");
            var Посмотреть = GetRuntimeHelperMethod("Посмотреть");
            инструции.Add(Instruction.Create(OpCodes.Call, Посмотреть));
            инструции.Add(Instruction.Create(OpCodes.Call, ТранслироватьПеременную));
            switch (команда)
            {
                case '$':
                    var ВывестиСимвол = GetRuntimeHelperMethod("ВывестиСимвол");
                    инструции.Add(Instruction.Create(OpCodes.Call, ВывестиСимвол));
                    break;
                case '?':
                    var ВывестиЧисло = GetRuntimeHelperMethod("ВывестиЧисло");
                    инструции.Add(Instruction.Create(OpCodes.Call, ВывестиЧисло));
                    break;
            }
        }
        else if (команда == '~')
        {
            var СнятьСоСтека = GetRuntimeHelperMethod("СнятьСоСтека");
            инструции.Add(Instruction.Create(OpCodes.Call, СнятьСоСтека));
            инструции.Add(Instruction.Create(OpCodes.Pop));
        }
        else if (команда == '%')
        {
            var ОчиститьСтек = GetRuntimeHelperMethod("ОчиститьСтек");
            инструции.Add(Instruction.Create(OpCodes.Call, ОчиститьСтек));
        }
        else if (команда == ':')
        {
            var СнятьСоСтека = GetRuntimeHelperMethod("СнятьСоСтека");
            var ПереключитьТекущийСтек = GetRuntimeHelperMethod("ПереключитьТекущийСтек");
            var СохранитьПеременную = GetRuntimeHelperMethod("СохранитьПеременную");
            инструции.Add(Instruction.Create(OpCodes.Call, СнятьСоСтека));
            инструции.Add(Instruction.Create(OpCodes.Call, ПереключитьТекущийСтек));
            инструции.Add(Instruction.Create(OpCodes.Call, СохранитьПеременную));
            инструции.Add(Instruction.Create(OpCodes.Call, ПереключитьТекущийСтек));
        }
        else if (команда == '#')
        {
            var ПереключитьТекущийСтек = GetRuntimeHelperMethod("ПереключитьТекущийСтек");
            инструции.Add(Instruction.Create(OpCodes.Call, ПереключитьТекущийСтек));
        }
        //else if (команда == '@')
        //{
        //    var кодМакроса = СнятьСоСтека();
        //    var макрос = ПолучитьМакрос(кодМакроса);
        //    макрос();
        //}
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
}
