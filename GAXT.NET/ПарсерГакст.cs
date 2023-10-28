namespace GAXT.NET;

using Yoakke.SynKit.Parser.Attributes;

using ТокенаГакст = Yoakke.SynKit.Lexer.IToken<ТипТокенаГакст>;

[Parser(typeof(ТипТокенаГакст))]
internal partial class ПарсерГакст
{
    [Rule($"Basic : OpCode+")]
    private static ПростойКод MakeOpCode(IReadOnlyList<ТокенаГакст> identifierParts)
    {
        return new ПростойКод(string.Join("", identifierParts.Select(_ => _.Text)));
    }

    [Rule($"StoreMacro : '(' Block ')'")]
    private static Макрос MakeStoreMacro(ТокенаГакст _, Выражение identifierParts, ТокенаГакст __)
    {
        return new Макрос(identifierParts);
    }

    [Rule($"LoopExpression : '[' Block ']'")]
    private static Цикл MakeLoopExpression(ТокенаГакст _, Выражение identifierParts, ТокенаГакст __)
    {
        return new Цикл(identifierParts);
    }

    [Rule($"ConditionalExpression : '{{' Block '|' Block '}}'")]
    private static Условие MakeConditionalExpression(ТокенаГакст _, Выражение thenExpression, ТокенаГакст __, Выражение elseExpression, ТокенаГакст ___)
    {
        return new Условие(thenExpression, elseExpression);
    }

    [Rule($"Expression : StoreMacro")]
    [Rule($"Expression : ConditionalExpression")]
    [Rule($"Expression : LoopExpression")]
    [Rule($"Expression : Basic")]
    private static Выражение MakeExpression(Выражение expression)
    {
        return expression;
    }

    [Rule($"Block : Expression*")]
    private static Блок MakeBlockExpression(IReadOnlyList<Выражение> identifierParts)
    {
        return new Блок(identifierParts);
    }

    [Rule($"Program : Block '!'")]
    private static Блок MakeProgram(Блок identifierParts, ТокенаГакст _)
    {
        return identifierParts;
    }
}

abstract record Выражение;
record ПростойКод(string код) : Выражение;
record Блок(IReadOnlyList<Выражение> Выражения) : Выражение;
record Макрос(Выражение Тело) : Выражение;
record Условие(Выражение thenExpression, Выражение elseExpression) : Выражение;
record Цикл(Выражение Тело) : Выражение;