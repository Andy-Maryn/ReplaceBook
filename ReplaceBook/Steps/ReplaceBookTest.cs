using NUnit.Framework;
using RestSharp;
using Newtonsoft.Json.Linq;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using Allure.Net.Commons;
using System;
using System.Linq;

namespace BookstoreTests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Bookstore API")]
    public class ReplaceBookTest
    {
        private RestClient _client;
        private string _userId;
        private string _token;
        private string _username;
        private string _password;

        [SetUp]
        public void Setup()
        {
            _client = new RestClient("https://bookstore.toolsqa.com");

            _username = $"user_{Guid.NewGuid().ToString().Substring(0, 8)}";
            _password = $"Pass@{Guid.NewGuid().ToString().Substring(0, 8)}";

            CreateUser();
            GenerateToken();
        }

        [TearDown]
        public void Cleanup()
        {
            if (_userId != null && _token != null)
            {
                DeleteUser();
            }

            if (_client != null)
            {
                _client.Dispose();
            }
        }

        [Test]
        [AllureTag("ReplaceBook")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Automation QA")]
        [AllureDescription("Replace a book in the user's list following structured test case steps")]
        public void ReplaceBookInUserList()
        {
            var (firstIsbn, secondIsbn) = GetTwoBookIsbns();
            AddBookToUser(firstIsbn);
            VerifyUserHasSingleBook(firstIsbn);
            ReplaceBook(firstIsbn, secondIsbn);
            VerifyUserHasSingleBook(secondIsbn);
        }

        [AllureStep("Create a new user for testing")]
        private void CreateUser()
        {
            var createUser = new RestRequest("/Account/v1/User", Method.Post);
            createUser.AddJsonBody(new { userName = _username, password = _password });
            var createResponse = _client.Execute(createUser);
            Assert.That(createResponse.IsSuccessful, Is.True, "User creation failed");

            _userId = JObject.Parse(createResponse.Content)["userID"].ToString();
        }

        [AllureStep("Generate authentication token")]
        private void GenerateToken()
        {
            var tokenRequest = new RestRequest("/Account/v1/GenerateToken", Method.Post);
            tokenRequest.AddJsonBody(new { userName = _username, password = _password });
            var tokenResponse = _client.Execute(tokenRequest);
            Assert.That(tokenResponse.IsSuccessful, Is.True, "Token generation failed");

            _token = JObject.Parse(tokenResponse.Content)["token"].ToString();
        }

        [AllureStep("Delete the test user")]
        private void DeleteUser()
        {
            var deleteRequest = new RestRequest($"/Account/v1/User/{_userId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {_token}");
            _client.Execute(deleteRequest);
        }

        [AllureStep("Fetch two ISBNs from the Bookstore")]
        private (string firstIsbn, string secondIsbn) GetTwoBookIsbns()
        {
            var getBooksRequest = new RestRequest("/BookStore/v1/Books", Method.Get);
            var getBooksResponse = _client.Execute(getBooksRequest);
            Assert.That(getBooksResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to get books");

            var books = JObject.Parse(getBooksResponse.Content)["books"];
            
            var firstIsbn = books[0]["isbn"].ToString();
            var secondIsbn = books[1]["isbn"].ToString();
            return (firstIsbn, secondIsbn);
        }

        [AllureStep("Add a book to the user library")]
        private void AddBookToUser(string isbn)
        {
            var addBookRequest = new RestRequest("/BookStore/v1/Books", Method.Post);
            addBookRequest.AddHeader("Authorization", $"Bearer {_token}");
            addBookRequest.AddJsonBody(new
            {
                userId = _userId,
                collectionOfIsbns = new[] { new { isbn } }
            });

            var addBookResponse = _client.Execute(addBookRequest);
            Assert.That(addBookResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created), "Failed to add book");
        }

        [AllureStep("Verify user has exactly one book with ISBN: {0}")]
        private void VerifyUserHasSingleBook(string expectedIsbn)
        {
            var getUserRequest = new RestRequest($"/Account/v1/User/{_userId}", Method.Get);
            getUserRequest.AddHeader("Authorization", $"Bearer {_token}");

            var getUserResponse = _client.Execute(getUserRequest);
            Assert.That(getUserResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), "Failed to get user");

            var userBooks = JObject.Parse(getUserResponse.Content)["books"];
            Assert.That(userBooks.Count(), Is.EqualTo(1), "User should have exactly one book");
            Assert.That(userBooks[0]["isbn"].ToString(), Is.EqualTo(expectedIsbn), $"Book ISBN mismatch, expected: {expectedIsbn}");
        }

        [AllureStep("Replace the book with ISBN: {0} with ISBN: {1}")]
        private void ReplaceBook(string oldIsbn, string newIsbn)
        {
            var replaceRequest = new RestRequest("/BookStore/v1/Books/{isbn}", Method.Put);
            replaceRequest.AddHeader("Authorization", $"Bearer {_token}");
            replaceRequest.AddUrlSegment("isbn", oldIsbn);
            replaceRequest.AddJsonBody(new
            {
                userId = _userId,
                isbn = newIsbn
            });

            var replaceResponse = _client.Execute(replaceRequest);
            Assert.That(replaceResponse.IsSuccessful, Is.True, "Failed to replace book");
        }
    }
}
