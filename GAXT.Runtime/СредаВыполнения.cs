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

    public static void ПоложитьВСтек(long значение)
    {
        текущийСтек.Push(значение);
    }

    public static void ПоложитьПеременную(long значение)
    {
        стекПеременных.Push(значение);
    }

    public static void ПереключитьТекущийСтек()
    {
        (текущийСтек, другойСтек) = (другойСтек, текущийСтек);
    }

    public static void СохранитьПеременную(long значение)
    {
        if (текущийСтек == стекПеременных)
        {
            другойСтек.Push(значение);
        }
        else
        {
            var оп1 = другойСтек.Peek();
            переменные[оп1] = значение;
            Debug.WriteLine($"Set variable {(char)(оп1 + 'a')} to значение {значение}");
        }
    }

    public static long Посмотреть()
    {
        var з = текущийСтек.Peek();
        if (текущийСтек == стекПеременных)
        {            
            return переменные.Length <= з ? з : переменные[з];
        }

        return з;
    }

    public static long СнятьСоСтека()
    {
        var b = текущийСтек.Pop();
        if (текущийСтек == стекПеременных)
        {
            return переменные[b];
        }

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
}
