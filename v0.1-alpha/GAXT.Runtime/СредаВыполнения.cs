using System.Diagnostics;

namespace GAXT.NET;

public static class СредаВыполнения
{
    static TextWriter писатель = Console.Out;
    static long[] переменные = new long['z' - 'a' + 1];
    static Stack<long> стекЗначений = new Stack<long>();
    static Stack<long> стекПеременных = new Stack<long>();
    static Stack<long> текущийСтек = стекЗначений;
    public static Stack<long> другойСтек = стекПеременных;
    static List<Action> списокМакросов = new List<Action>();

    internal static void УстановитьУстройствоВывода(TextWriter новыйПисатель)
    {
        писатель = новыйПисатель;
    }

    internal static void СброситьОкружение()
    {
        стекЗначений.Clear();
        стекПеременных.Clear();
        текущийСтек = стекЗначений;
        другойСтек = стекПеременных;
        списокМакросов.Clear();
        переменные = new long['z' - 'a' + 1];
        писатель = Console.Out;
    }

    public static void ПоложитьВСтек(long значение)
    {
        текущийСтек.Push(значение);
    }

    public static void ПоложитьПеременную(char переменная)
    {
        стекПеременных.Push(переменная - 'a');
    }

    public static void ПереключитьТекущийСтек()
    {
        var врм = текущийСтек;
        текущийСтек = другойСтек;
        другойСтек = врм;
    }

    public static void СохранитьПеременную(long значение)
    {
        if (текущийСтек == стекЗначений)
        {
            текущийСтек.Pop();
            текущийСтек.Push(значение);
        }
        else
        {
            var оп1 = текущийСтек.Peek();
            переменные[оп1] = значение;
            Debug.WriteLine($"Set variable {(char)(оп1 + 'a')} to значение {значение}");
        }
    }

    public static long ТранслироватьПеременную(long значение)
    {
        if (текущийСтек == стекПеременных)
        {
            return переменные[значение];
        }

        return значение;
    }

    public static long СклеитьЗначения(long топ1, long топ2)
    {
        var sign1 = Math.Abs(топ1) == топ1 ? 1 : -1;
        var sign2 = Math.Abs(топ2) == топ2 ? 1 : -1;
        var значение = sign1 * sign2 * long.Parse(Math.Abs(топ1).ToString() + Math.Abs(топ2).ToString());
        return значение;
    }

    internal static long ЗначениеПеременной(char значение)
    {
        return переменные[(char)значение - 'a'];
    }

    public static long Посмотреть()
    {
        var з = текущийСтек.Peek();
        return з;
    }

    public static long СнятьСоСтека()
    {
        var b = текущийСтек.Pop();
        return b;
    }

    public static void ОчиститьСтек()
    {
        текущийСтек.Clear();
    }

    public static void ВывестиСимвол(long значение)
    {
        писатель.Write((char)значение);
    }

    public static void ВывестиЧисло(long значение)
    {
        писатель.Write(значение);
    }

    public static void ЗарегистрироватьМакрос(Action макрос)
    {
        списокМакросов.Add(макрос);
    }

    public static Action ПолучитьМакрос(long кодМакроса)
    {
        return списокМакросов[(int)кодМакроса];
    }

    internal static void DumpInterpreterState()
    {
        Console.WriteLine($"Текущий стек: {(текущийСтек == стекЗначений ? "значений" : "переменных")}");
        Console.WriteLine($"Стек значений: {string.Join(",", стекЗначений)}");
        var переменные = string.Join(",", стекПеременных.Select(_ => (char)(_ + 'a')));
        var значенияПеременных = string.Join(",", стекПеременных.Select(_ => ЗначениеПеременной((char)(_ + 'a'))));
        Console.WriteLine($"Стек переменных: {переменные} | {значенияПеременных}");
    }
}
