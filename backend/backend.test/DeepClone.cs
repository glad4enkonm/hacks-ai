using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using database.Helpers;

namespace backend.test;

[TestFixture]
public class DeepClone
{
    private const int NumIterations = 100000;
    
    private void AssertObjectsAreEqual(MyClass expected, MyClass actual)
    {
        Assert.That(actual.Id, Is.EqualTo(expected.Id));
        Assert.That(actual.Name, Is.EqualTo(expected.Name));
        Assert.That(actual.DateOfBirth, Is.EqualTo(expected.DateOfBirth));

        Assert.That(actual.Addresses.Count, Is.EqualTo(expected.Addresses.Count));
        for (int i = 0; i < expected.Addresses.Count; i++)
        {
            Assert.That(actual.Addresses[i].Street, Is.EqualTo(expected.Addresses[i].Street));
            Assert.That(actual.Addresses[i].City, Is.EqualTo(expected.Addresses[i].City));
            Assert.That(actual.Addresses[i].State, Is.EqualTo(expected.Addresses[i].State));
            Assert.That(actual.Addresses[i].ZipCode, Is.EqualTo(expected.Addresses[i].ZipCode));
        }

        Assert.That(actual.PhoneNumbers.Count, Is.EqualTo(expected.PhoneNumbers.Count));
        for (int i = 0; i < expected.PhoneNumbers.Count; i++)
        {
            Assert.That(actual.PhoneNumbers[i].Number, Is.EqualTo(expected.PhoneNumbers[i].Number));
            Assert.That(actual.PhoneNumbers[i].Type, Is.EqualTo(expected.PhoneNumbers[i].Type));
        }

        Assert.That(actual.Salary.Amount, Is.EqualTo(expected.Salary.Amount));
        Assert.That(actual.Salary.Currency, Is.EqualTo(expected.Salary.Currency));

        expected.Addresses.First().ZipCode = "000"; // проверяем что действительно глубокая копия
        Assert.That(actual.Addresses.First().ZipCode, Is.Not.EqualTo("000"));
        
    }

    private MyClass CreateOriginal()
    {
        var address1 = new Address { Street = "123 Main St", City = "Anytown", State = "CA", ZipCode = "12345" };
        var address2 = new Address { Street = "456 Elm St", City = "Othertown", State = "CA", ZipCode = "67890" };
        var phone1 = new PhoneNumber { Number = "555-1234", Type = "Home" };
        var phone2 = new PhoneNumber { Number = "555-5678", Type = "Work" };
        var salary = new Salary { Amount = 50000m, Currency = "USD" };

        var employee = new MyClass
        {
            Id = 1,
            Name = "John Smith",
            DateOfBirth = new DateTime(1980, 1, 1),
            Addresses = new List<Address> { address1, address2 },
            PhoneNumbers = new List<PhoneNumber> { phone1, phone2 },
            Salary = salary
        };

        return employee;
    }
    
    private void TestCopyMethod(MyClass original, string methodName, Func<MyClass> copyMethod)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        for (int i = 0; i < NumIterations; i++)
        {
            var copy = copyMethod();
            AssertObjectsAreEqual(copy, original);
        }

        stopwatch.Stop();
        Console.WriteLine($"{methodName} took {stopwatch.ElapsedMilliseconds} ms to complete {NumIterations} iterations");
    }

    [Test]
    public void TestMethod1()
    {
        var original = CreateOriginal();
        TestCopyMethod(original, "Method 1", () => original.CopyMethod1());
    }
    
    
    [Test]
    public void TestMethod2()
    {
        var original = CreateOriginal();
        TestCopyMethod(original, "Method 2", () => original.Copy());
    }
}

[Serializable]
public class MyClass
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public List<Address> Addresses { get; set; }
    public List<PhoneNumber> PhoneNumbers { get; set; }
    public Salary Salary { get; set; }
}

[Serializable]
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

[Serializable]
public class PhoneNumber
{
    public string Number { get; set; }
    public string Type { get; set; }
}

[Serializable]
public class Salary
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}

public static class CopyExtensions
{
    [Obsolete("Obsolete")]
    public static T CopyMethod1<T>(this T original)
    {
        using var ms = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(ms, original);
        ms.Position = 0;
        return (T)formatter.Deserialize(ms);
    }
}