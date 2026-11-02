module Тесты

open System
open Xunit
open GAXT.NET

let проверитьПрограмму (программа: string) (ожидаемыйВывод: string) =
    let консоль = new System.IO.StringWriter()
    let интерпретатор = Программа.создатьИнтерпретатор консоль
    let результат = Программа.интерпретироватьСтроку интерпретатор программа
    Assert.Equal(ожидаемыйВывод, консоль.ToString())

[<Fact>]
let ``Hello, world!`` () =
    проверитьПрограмму "\"Hello, world!'\\n\"~[$~]!" "Hello, world!\n"

[<Fact>]
let ``Арифметика`` () =
    проверитьПрограмму "123++?!" "6"
