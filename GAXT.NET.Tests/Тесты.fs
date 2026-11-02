module Тесты

open System
open Xunit
open GAXT.NET

[<Fact>]
let ``Hello, world!`` () =
    let программа = "\"Hello, world!'\\n\"~[$~]!";
    let консоль = new System.IO.StringWriter()
    let интерпретатор = Программа.создатьИнтерпретатор консоль
    let результат = Программа.интерпретироватьСтроку интерпретатор программа
    Assert.Equal("Hello, world!\n", консоль.ToString())
