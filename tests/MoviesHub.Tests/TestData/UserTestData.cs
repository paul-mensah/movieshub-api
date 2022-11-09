using Bogus;
using MoviesHub.Api.Storage.Entities;

namespace MoviesHub.Tests.TestData;

public static class UserTestData
{
    private static readonly Faker Faker = new Faker();

    public static List<User> GenerateUsers(int count)
    {
        var usersList = new List<User>();

        for (int i = 0; i < count; i++)
        {
            usersList.Add(new User
            {
                FirstName = Faker.Person.FirstName,
                LastName = Faker.Person.LastName,
                MobileNumber = Faker.Person.Phone
            });
        }

        return usersList;
    }
}