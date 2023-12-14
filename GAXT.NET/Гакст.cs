﻿using static GAXT.NET.Интерпретатор;
using static GAXT.NET.Компилятор;

var приветМирДлинный = $@"72_$~
J1+$~
10_8_$~
10_8_$~
11_1_$~
44_$~
32_$~
11_9_$~
11_1_$~
11_4_$~
10_8_$~
J$~
33_$~!
";
var приветМирДельта = $@"G2+$
C+1-$
7+$$
3+$
G-3+$
A-2-$
JB+1-$
8-$
3+$
6-$
8-$
F-7-$~!
";
var программаСУсловиями = $@"12>
{{
    42+
    |
    56=
    {{
        3?
        |
        47<
        {{
            4?
            |
            7?
        }}
    }}
}}!
";
var программаСМакросами = $@"
(a0:b0:)            обнулить а и б
(#?~#)              напечатать значение вершины другого стека
(C2+$~)             напечатать пробел
($~ 2@ F1+$~ 2@)    напечатать символ и пробел  и равно и пробел


a3:                 а равно трём
I7+ 3@ a1@ 2@       напечатать а равно и его значение и пробел

b5:                 б равно пяти
I8+ 3@ b1@          напечатать б равно и его значение

0@                  очистить а и б
A$                  напечатать перевод строки

I7+ 3@ a1@ 2@       напечатать а равно и его значение и пробел
I8+ 3@ b1@          напечатать б равно и его значение
!                   финиш
";
var программаСЦиклами = $@"
An: 0x: 1y: 0z: 0i: 1b: #%
x
[
  ~
  ib+~
  x?~ # C2+$~ 

  0z:x#+y+~#
  0x:y#+~#
  0y:z#+~#
  0t:i#+~
  tn<
]~!
";
var программаСЦиклами2 = $@"
9n:
0i:
1b:
#
~
[
  ~~
  i?
  b+
  #0t:i#+~
  tn<
]~~!
";
var программа99бутылокСПивом = $@"
(
    9@
    H3+-$
    H1-+$ 9-$
    G-$
    F6++$ 3+$$ A3++$%
)

(
    C2+$
    G9++$ 1-$
    G8+-$
    H4++$ A2+-$ 3-$
    G1--$
    H7++$ B2+-$ A1++$$%
)

(
    H4+$ A3++$ A+$ 6-$
    F9+-$
    G9++$ 1-$ 9-$
    F9+-$
    F8++$ A1++$ 8+$ 9-$
    G8+-$
    F5++$ A3++$ A-$
    F8+-$
    H+$ A5+-$ A8++$$
    H3+-$
    G3++$ A1++$
    H4+-$
    F5++$ A7++$ 3-$ 6+$ 7-$ A-$
    E6+-$ A2+-$%
)

(
    I8+$ A3++$ 5+$$ 8-$ 7-$
    F9+-$
    G9++$ 9-$
    G-$
    F6++$ 3+$$ A3++$%
)

(0@1@)
(#ba+?ba-%#)
(D4+$ A2+-$%)
(D9+$ A7+-$%)
(D6+$C6+-$%)

(
    C2+$
    F6++$ A3++$ 5+$$ 8-$ 7-$ A4++$
)

(
    5@4@6@5@0@ 8@
    2@#b?#4@8@ #ba-#
)

(
    JA+$ 1+$
    G9+-$
    G7++$ 2+$ 3+$ A3+-$%
)

98_b: a1:
b#[#A@#]#

7@3@1@6@7@3@8@
2@A1+@4@!
";
/*
A,B = константы

k,l,m - переменные

b,t - временные переменные используемые компилятором b для константы, t для переменных

k = k + A

Ak:

k = A + B

AB+k:

k = k + l
kl:

k = l + A
Ak:l#+#

k = l + m
Am#:#k:l#+#

*/

var программа = программа99бутылокСПивом;
программа = @"
(
простой случай
#a:#3<1#a:#\
рекурсивный случай
#ab-#0@1-0@~+
)
#1b:
0@

!";
//программа = программаСЦиклами2;
//программа = программаСЦиклами;
//программа = программаСМакросами;
//программа = программаСУсловиями;
//программа = приветМирДельта;
//программа = приветМирДлинный;
if (args.Length > 0)
{
    var имяФайла = args[0];
    if (File.Exists(имяФайла))
    {
        программа = File.ReadAllText(имяФайла);
    }
    else
    {
        Console.WriteLine($"Файл {имяФайла} не существует");
        return;
    }
}

Скомпилировать("app", программа);
Выполнить(программа.ToString());