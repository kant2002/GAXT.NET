namespace GAXT.NET;

using Yoakke.SynKit.Lexer;
using Yoakke.SynKit.Lexer.Attributes;

internal enum ТипТокенаГакст
{
    [Error] Ошибка,
    [End] Конец,
    [Ignore][Regex(Regexes.Whitespace)] Whitespace,
    [Ignore][Regex("[а-яА-ЯёЁо]")] Комментарии,

    [Regex("[A-Za-z0-9-+*/_<>=$?~:#@%]")] Операция,
    [Token("{")] НачалоУсловия,
    [Token("|")] Иначе,
    [Token("}")] КонецУсловия,
    [Token("[")] НачалоЦикла,
    [Token("]")] КонецЦикла,
    [Token("!")] ЗавершениеПрограммы,
    [Token("\\")] Прервать,
}
