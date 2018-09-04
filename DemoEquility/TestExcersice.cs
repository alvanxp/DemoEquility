using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTestProject1
{
    [TestClass]
    public class TestExcersice
    {
        [TestMethod]
        public void Test()
        {
            var address = new Address("5455 Apache Trail", "Queen Creek", "AZ", "85243");
            var person = new Person("Jane", "Smith", address);
            var biz = new Business("Alliance Reservations Network", address);
            Assert.IsTrue(string.IsNullOrEmpty(person.Id));
            person.Save();
            Assert.IsTrue(!string.IsNullOrEmpty(person.Id));

            Assert.IsTrue(string.IsNullOrEmpty(biz.Id));
            biz.Save();
            Assert.IsTrue(!string.IsNullOrEmpty(biz.Id));

            Person savedPerson = Person.Find(person.Id);
            Assert.IsTrue(person == savedPerson);
            Assert.IsNotNull(savedPerson);
            Assert.AreSame(person.Address, address);
            Assert.AreEqual(savedPerson.Address, address);
            Assert.AreEqual(person.Id, savedPerson.Id);
            Assert.AreEqual(person.FirstName, savedPerson.FirstName);
            Assert.AreEqual(person.LastName, savedPerson.LastName);
            Assert.AreEqual(person, savedPerson);
            Assert.AreNotSame(person, savedPerson);
            Assert.AreNotSame(person.Address, savedPerson.Address);

            Business savedBusiness = Business.Find(biz.Id);
            Assert.IsNotNull(savedBusiness);
            Assert.AreSame(biz.Address, address);
            Assert.AreEqual(savedBusiness.Address, address);
            Assert.AreEqual(biz.Id, savedBusiness.Id);
            Assert.AreEqual(biz.Name, savedBusiness.Name);
            Assert.AreEqual(biz, savedBusiness);
            Assert.AreNotSame(biz, savedBusiness);
            Assert.AreNotSame(biz.Address, savedBusiness.Address);

            var dictionary = new Dictionary { [address] = address, [person] = person, [biz] = biz };
            Assert.IsTrue(dictionary.ContainsKey(new Address("5455 Apache Trail", "Queen Creek", "AZ", "85243")));
            Assert.IsTrue(dictionary.ContainsKey(new Person("Jane", "Smith", address)));
            Assert.IsTrue(dictionary.ContainsKey(new Business("Alliance Reservations Network", address)));
            Assert.IsFalse(dictionary.ContainsKey(new Address("54553 Apache Trail", "Queen Creek", "AZ", "85243")));
            Assert.IsFalse(dictionary.ContainsKey(new Person("Jim", "Smith", address)));
            Assert.IsFalse(dictionary.ContainsKey(new Business("Alliance Reservation Networks", address)));

            person.Delete();
            Assert.IsTrue(string.IsNullOrEmpty(person.Id));
            Assert.IsNull(Person.Find(person.Id));

            biz.Delete();
            Assert.IsTrue(string.IsNullOrEmpty(biz.Id));
            Assert.IsNull(Person.Find(biz.Id));
        }
    }

    public class Dictionary : Dictionary<PersistenceEntity, PersistenceEntity>
    {

    }

    public abstract class PersistenceEntity
    {
        private const string Dir = @"C:\temp\";
        public string Id { get; set; }

        public void Save()
        {
            Id = Guid.NewGuid().ToString();
            var serializedObject = JsonConvert.SerializeObject(this);
            SaveObject(serializedObject);
        }

        private void SaveObject(string dataText)
        {
            bool exists = System.IO.Directory.Exists(Dir);
            if (!exists)
                System.IO.Directory.CreateDirectory(Dir);
            using (StreamWriter writer = new StreamWriter(Path))
            {
                writer.WriteLine(dataText);
            }
        }

        private void SaveObjects(string dataText)
        {
            bool exists = System.IO.Directory.Exists(Dir);
            if (!exists)
                System.IO.Directory.CreateDirectory(Dir);
            using (FileStream fs = File.Create(Path))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(dataText);
                fs.Write(info, 0, info.Length);
                byte[] data = new byte[] { 0x0 };
                fs.Write(data, 0, data.Length);
            }
        }

        public static T Find<T>(string entityId) where T : PersistenceEntity
        {
            string entityName = typeof(T).Name;
            string path = $"{Dir}{entityName}";
            string line = null;
            using (StreamReader reader = new StreamReader(path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    var deserializeObject = JsonConvert.DeserializeObject<T>(line);
                    var entity = deserializeObject as PersistenceEntity;
                    if (entity != null && entity.Id == entityId)
                        return deserializeObject;
                }
            }
            return null;
        }

        public void Delete()
        {
            var entitiesText = File.ReadAllLines(Path);
            StringBuilder result = new StringBuilder();
            foreach (var entityText in entitiesText)
            {
                var deserializeObject = JsonConvert.DeserializeObject(entityText, this.GetType());
                var entity = deserializeObject as PersistenceEntity;
                if (entity != null && Id != entity.Id)
                    result.AppendLine(entityText);
            }
            SaveObjects(result.ToString());
            Id = null;
        }

        private string Path
        {
            get
            {
                string entityName = this.GetType().Name;
                return $"{Dir}{entityName}";
            }
        }
    }

    public class Business : PersistenceEntity
    {
        private readonly string _name;
        private readonly Address _address;

        public Business(string name, Address address)
        {
            _name = name;
            _address = address;
        }

        public string Name => _name;

        public Address Address => _address;

        public static Business Find(string personId)
        {
            return Find<Business>(personId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            Business business = (Business)obj;
            return string.Equals(_name, business._name) && Equals(_address, business._address);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0) * 397) ^
                       (_address != null ? _address.GetHashCode() : 0);
            }
        }
    }

    public class Person : PersistenceEntity
    {
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly Address _address;

        public Person(string firstName, string lastName, Address address)
        {
            _firstName = firstName;
            _lastName = lastName;
            _address = address;
        }

        public string FirstName => _firstName;

        public string LastName => _lastName;

        public Address Address => _address;

        public static Person Find(string personId)
        {
            return Find<Person>(personId);
        }

        public static bool operator ==(Person x, Person y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(Person x, Person y)
        {
            return !Equals(x, y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            Person person = (Person)obj;
            return string.Equals(_firstName, person._firstName) && string.Equals(_lastName, person._lastName) &&
                       Equals(_address, person._address);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_firstName != null ? _firstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_lastName != null ? _lastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_address != null ? _address.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class Address : PersistenceEntity
    {
        private readonly string _street;
        private readonly string _city;
        private readonly string _state;
        private readonly string _zipCode;

        public Address(string street, string city, string state, string zipCode)
        {
            _street = street;
            _city = city;
            _state = state;
            _zipCode = zipCode;
        }

        public string Street => _street;

        public string City => _city;

        public string State => _state;

        public string ZipCode => _zipCode;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            Address address = (Address)obj;
            return string.Equals(_street, address._street) && string.Equals(_city, address._city) &&
                       string.Equals(_state, address._state) && string.Equals(_zipCode, address._zipCode);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_street != null ? _street.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_city != null ? _city.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_state != null ? _state.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_zipCode != null ? _zipCode.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}
