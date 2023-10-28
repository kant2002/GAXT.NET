using GAXT.NET;
using static GAXT.NET.Интерпретатор;
using static GAXT.NET.СредаВыполнения;

namespace GAXT.Tests;

public class ТестыИнтерпретатора
{
    StringWriter писатель = new();
    public ТестыИнтерпретатора()
    {
        СброситьОкружение();
        УстановитьУстройствоВывода(писатель);
    }

    [Fact]
    public void ПечатьЧисла()
    {
        Выполнить("1?!");

        Assert.Equal("1", писатель.ToString());
    }

    [Fact]
    public void Константы()
    {
        Выполнить("A?!");

        Assert.Equal("10", писатель.ToString());
    }

    [Fact]
    public void СложениеЗначений()
    {
        Выполнить("12+?!");

        Assert.Equal("3", писатель.ToString());
    }

    [Fact]
    public void ВычитаниеЗначений()
    {
        Выполнить("12-?!");

        Assert.Equal("-1", писатель.ToString());
    }

    [Fact]
    public void ПрисваиваниеЗначений()
    {
        Выполнить("Ai:!");

        Assert.Equal(10, ЗначениеПеременной('i'));
    }

    [Fact]
    public void СравнениеПеременных()
    {
        Выполнить("Ai:i#=?!");

        Assert.Equal("1", писатель.ToString());
        Assert.Equal(1, ЗначениеПеременной('i'));
    }
}