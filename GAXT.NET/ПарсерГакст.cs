using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAXT.NET;

using Yoakke.SynKit.Lexer;
using Yoakke.SynKit.Parser;
using Yoakke.SynKit.Parser.Attributes;

using ТокенаГакст = Yoakke.SynKit.Lexer.IToken<ТипТокенаГакст>;

[Parser(typeof(ТипТокенаГакст))]
internal partial class ПарсерГакст
{
    [Rule($"Basic : OpCode+")]
    private static PlainProgram MakeOpCode(IReadOnlyList<ТокенаГакст> identifierParts)
    {
        return new PlainProgram(string.Join("", identifierParts.Select(_ => _.Text)));
    }

    [Rule($"StoreMacro : '(' Block ')'")]
    private static StoreMacro MakeStoreMacro(ТокенаГакст _, Expression identifierParts, ТокенаГакст __)
    {
        return new StoreMacro(identifierParts);
    }

    [Rule($"LoopExpression : '(' Block ')'")]
    private static LoopExpression MakeLoopExpression(ТокенаГакст _, Expression identifierParts, ТокенаГакст __)
    {
        return new LoopExpression(identifierParts);
    }

    [Rule($"ConditionalExpression : '{{' Block '|' Block '}}'")]
    private static ConditionalExpression MakeConditionalExpression(ТокенаГакст _, Expression thenExpression, ТокенаГакст __, Expression elseExpression, ТокенаГакст ___)
    {
        return new ConditionalExpression(thenExpression, elseExpression);
    }

    [Rule($"Expression : StoreMacro")]
    [Rule($"Expression : ConditionalExpression")]
    [Rule($"Expression : LoopExpression")]
    [Rule($"Expression : Basic")]
    private static Expression MakeExpression(Expression expression)
    {
        return expression;
    }

    [Rule($"Block : Expression*")]
    private static BlockExpression MakeBlockExpression(IReadOnlyList<Expression> identifierParts)
    {
        return new BlockExpression(identifierParts);
    }

    [Rule($"Program : Block '!'")]
    private static BlockExpression MakeProgram(BlockExpression identifierParts, ТокенаГакст _)
    {
        return identifierParts;
    }
}

abstract record Expression;
record PlainProgram(string code) : Expression;
record BlockExpression(IReadOnlyList<Expression> code) : Expression;
record StoreMacro(Expression code) : Expression;
record ConditionalExpression(Expression thenExpression, Expression elseExpression) : Expression;
record LoopExpression(Expression code) : Expression;