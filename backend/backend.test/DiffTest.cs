using System.Text.Json;
using database.Helpers;

namespace backend.test;

internal class Io1
{
    public int A { get; set; }
    public string B { get; set; }
    public int C { get; set; }
    public int X { get; set; }
}

internal class Io2
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
}

internal class NullableDateTime
{
    public DateTime? DateTime { get; set; }
}

internal class GetDifferentPropertiesTestObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Value { get; set; }
}

public class Tests
{
    [Test]
    public void EqualObjectsDiffIsEmpty()
    {
        var o1 = new Io2{
            A = 1,
            B = 2,
            C = 3
        };
        
        var o2 = new List<KeyValuePair<string, string>>() {
            new("d", "1"),
            new("b", "2"),
            new("c", "3"),
        };
        
        var r = Diff.GetAllowedDiff(o1, o2, new string[]{"B","C"}).ToList();
        Assert.That(r, Is.Empty);
    }
    
    [Test]
    public void OneAllowedValueIsDifferent()
    {
        var o1 = new Io2{
            A = 1,
            B = 2,
            C = 3
        };
        
        var o2 = new List<KeyValuePair<string, string>>() {
            new("d", "1"),
            new("b", "20"),
            new("c", "3"),
        };
        
        var r = Diff.GetAllowedDiff(o1, o2, new string[]{"B","C"});
        Assert.That(r.Any(pair => pair.Key == "B" && (int)pair.Value == 20), Is.True);
    }
    
    [Test]
    public void OneAllowedValueIsDifferentString()
    {
        var o1 = new Io1{
            A = 1,
            B = "2",
            C = 3,
            X = 2
        };
        
        var o2 = new List<KeyValuePair<string, string>>() {
            new("d", "1"),
            new("b", "20"),
            new("c", "3"),
        };

        var r = Diff.GetAllowedDiff(o1, o2, new string[]{"B","C"});
        Assert.That(r.Any(pair => pair.Key == "B" && (string)pair.Value == "20"), Is.True);
    }
    
    [Test]
    public void OneAllowedValueIsSameString()
    {
        var o1 = new Io1{
            A = 1,
            B = "20",
            C = 3,
            X = 43
        };
        
        var o2 = new List<KeyValuePair<string, string>>() {
            new("d", "1"),
            new("b", "20"),
            new("c", "3"),
        };
        
        var r = Diff.GetAllowedDiff(o1, o2, new string[]{"B","C"}).ToList();
        Assert.That(r, Is.Empty);
    }
    
    [Test]
    public void ApplyAllowedWithNoDifferenceIsOriginal()
    {
        var o1 = new Io1 {
            A = 1,
            B = "20",
            C = 3,
            X = 9
        };

        var o2 = new List<KeyValuePair<string, string>>() {
            new("d", "1"),
            new("b", "20"),
            new("c", "3"),
        };
        
        var r = Diff.ApplyAllowedDiff(o1, o2, new string[]{"B","C"});
        Assert.That(r, Is.EqualTo(o1));
    }
    
    [Test]
    public void ApplyAllowedWithDifferenceIsCorrect()
    {
        var o1 = new Io1() {
            A = 1,
            B = "20",
            C = 3,
            X = 1
        };
        
        var o2 = new List<KeyValuePair<string, string>>() {
            new("d", "1"),
            new("b", "21"),
            new("c", "33"),
            new("x", "3"),
        };

        var expected = new Io1()
        {
            A = 1,
            X = 1,
            B = "21",
            C = 33
        };
        
        var r = Diff.ApplyAllowedDiff(o1, o2, new string[]{"B","C"});
        Assert.That(JsonSerializer.Serialize(r), Is.EqualTo(JsonSerializer.Serialize(expected)));
    }
    
    [Test]
    public void ApplyAllowedWithDifferenceIsCorrectWithNullableDateTime()
    {
        var dt1 = new NullableDateTime() {
            DateTime = null
        };
        
        var diff = new List<KeyValuePair<string, string>>() {
            new("DateTime", "Sat Apr 29 2023 20:04:15 GMT+0300 (Moscow Standard Time)"),
        };

        var r = Diff.ApplyAllowedDiff(dt1, diff, new string[]{"DateTime"});
        Assert.That(((NullableDateTime)r).DateTime.HasValue, Is.EqualTo(true));
    }
    
    [Test]
    public void ApplyAllowedWithDifferenceIsCorrectWithNullableDateTimeReset()
    {
        var dt1 = new NullableDateTime() {
            DateTime = DateTime.Now
        };
        
        var diff = new List<KeyValuePair<string, string>>() {
            new("DateTime", ""),
        };

        var r = Diff.ApplyAllowedDiff(dt1, diff, new string[]{"DateTime"});
        Assert.That(((NullableDateTime)r).DateTime.HasValue, Is.EqualTo(false));
    }
    
    [Test]
    public void GetDifferentProperties_ShouldReturnCorrectProperties()
    {
        // Arrange
        var original = new GetDifferentPropertiesTestObject { Id = 1, Name = "Test", Value = 1.23 };
        var updated = new GetDifferentPropertiesTestObject { Id = 2, Name = "Updated", Value = 1.23 };

        // Act
        List<KeyValuePair<string, string>> differentProps = Diff.GetDifferentProperties(original, updated);

        // Assert
        Assert.That(differentProps.Count, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(differentProps[0].Key, Is.EqualTo("Id"));
            Assert.That(differentProps[0].Value, Is.EqualTo("2"));
            Assert.That(differentProps[1].Key, Is.EqualTo("Name"));
            Assert.That(differentProps[1].Value, Is.EqualTo("\"Updated\""));
        });
    }

    [Test]
    public void GetDifferentProperties_ShouldNotIncludeUnchangedProperties()
    {
        // Arrange
        var original = new GetDifferentPropertiesTestObject { Id = 1, Name = "Test", Value = 1.23 };
        var updated = new GetDifferentPropertiesTestObject { Id = 1, Name = "Test", Value = 1.23 };

        // Act
        List<KeyValuePair<string, string>> differentProps = Diff.GetDifferentProperties(original, updated);

        // Assert
        Assert.That(differentProps, Is.Empty);
    }
    
    [Test]
    public void GetDifferentProperties_ShouldIncludeNullProperties()
    {
        // Arrange
        var original = new GetDifferentPropertiesTestObject { Id = 1, Name = "Test", Value = 1.23 };
        var updated = new GetDifferentPropertiesTestObject { Id = 1, Name = null, Value = 1.23 };

        // Act
        List<KeyValuePair<string, string>> differentProps = Diff.GetDifferentProperties(original, updated);

        // Assert
        Assert.That(differentProps.Count, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(differentProps[0].Key, Is.EqualTo("Name"));
            Assert.That(differentProps[0].Value, Is.EqualTo("null"));
        });
    }
}