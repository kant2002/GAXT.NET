namespace GAXT.NET;

using Yoakke.SynKit.Lexer;
using Yoakke.SynKit.Lexer.Attributes;

internal enum ТипТокенаГакст
{
    [Error] Error,
    [End] End,
    [Ignore][Regex(Regexes.Whitespace)] Whitespace,
    [Ignore][Regex("[а-яА-ЯёЁо]")] Комментарии,

    [Regex("[A-Za-z0-9-+*/_<>=$?~:#@]")] OpCode,
    [Token("{")] BeginIf,
    [Token("|")] Else,
    [Token("}")] EndIf,
    [Token("!")] FinishProgram,
}
