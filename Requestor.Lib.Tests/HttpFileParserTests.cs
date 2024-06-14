using System.Text;

namespace Requestor.Lib.Tests;

public class HttpFileParserTests
{
    [Test]
    public async Task TestParse1()
    {
        byte[] data = Encoding.UTF8.GetBytes("GET http://host/p/ath");

        var sut = new HttpFileParser();

        var result = await sut.ParseAsync(new MemoryStream(data));

        Assert.That(result.Variables, Is.Empty);
        Assert.That(result.Requests.Count, Is.EqualTo(1));

        var request = result.Requests[0];

        Assert.That(request.Verb, Is.EqualTo(RequestVerb.Get));
        Assert.That(request.Url, Is.EqualTo("http://host/p/ath"));
        Assert.That(request.HttpVersion, Is.Null);
        Assert.That(request.Headers, Is.Empty);
        Assert.That(request.RequestBody, Is.EqualTo(""));
        Assert.That(request.Variables, Is.Empty);
    }

    [Test]
    public async Task TestParse2()
    {
        byte[] data = Encoding.UTF8.GetBytes("http://host/p/ath");

        var sut = new HttpFileParser();

        var result = await sut.ParseAsync(new MemoryStream(data));

        Assert.That(result.Variables, Is.Empty);
        Assert.That(result.Requests.Count, Is.EqualTo(1));

        var request = result.Requests[0];

        Assert.That(request.Verb, Is.EqualTo(RequestVerb.Get));
        Assert.That(request.Url, Is.EqualTo("http://host/p/ath"));
        Assert.That(request.HttpVersion, Is.Null);
        Assert.That(request.Headers, Is.Empty);
        Assert.That(request.RequestBody, Is.EqualTo(""));
        Assert.That(request.Variables, Is.Empty);
    }

    [Test]
    public async Task TestParse3()
    {
        byte[] data = Encoding.UTF8.GetBytes("""
            GET https://example.com/comments/1?page=2&pageSize=10
            User-Agent: rest-client
            Accept-Language: en-GB,en-US;q=0.8,en;q=0.6,zh-CN;q=0.4
            Content-Type: application/json
            """);

        var sut = new HttpFileParser();

        var result = await sut.ParseAsync(new MemoryStream(data));

        Assert.That(result.Variables, Is.Empty);
        Assert.That(result.Requests.Count, Is.EqualTo(1));

        var request = result.Requests[0];

        Assert.That(request.Verb, Is.EqualTo(RequestVerb.Get));
        Assert.That(request.Url, Is.EqualTo("https://example.com/comments/1?page=2&pageSize=10"));
        Assert.That(request.HttpVersion, Is.Null);
        Assert.That(request.Headers, Contains.Key("User-Agent").WithValue("rest-client"));
        Assert.That(request.Headers, Contains.Key("Accept-Language").WithValue("en-GB,en-US;q=0.8,en;q=0.6,zh-CN;q=0.4"));
        Assert.That(request.Headers, Contains.Key("Content-Type").WithValue("application/json"));
        Assert.That(request.RequestBody, Is.EqualTo(""));
        Assert.That(request.Variables, Is.Empty);
    }

    [Test]
    public async Task TestParse4()
    {
        byte[] data = Encoding.UTF8.GetBytes("""
            DELETE https://example.com/comments/1
            ?page=2
            &pageSize=10
            """);

        var sut = new HttpFileParser();

        var result = await sut.ParseAsync(new MemoryStream(data));

        Assert.That(result.Variables, Is.Empty);
        Assert.That(result.Requests.Count, Is.EqualTo(1));

        var request = result.Requests[0];

        Assert.That(request.Verb, Is.EqualTo(RequestVerb.Delete));
        Assert.That(request.Url, Is.EqualTo("https://example.com/comments/1?page=2&pageSize=10"));
        Assert.That(request.HttpVersion, Is.Null);
        Assert.That(request.Headers, Is.Empty);
        Assert.That(request.RequestBody, Is.EqualTo(""));
        Assert.That(request.Variables, Is.Empty);
        Assert.That(request.Comment, Is.Null);
    }

    [Test]
    public async Task TestParse5()
    {
        byte[] data = Encoding.UTF8.GetBytes("""
            ### Create a new item

            POST https://localhost:5167/todoitems
            Content-Type: application/json

            {
                "id": "id",
                "name":"walk dog",
                "isComplete":false
            }
            """);

        var sut = new HttpFileParser();

        var result = await sut.ParseAsync(new MemoryStream(data));

        Assert.That(result.Variables, Is.Empty);
        Assert.That(result.Requests.Count, Is.EqualTo(1));

        var request = result.Requests[0];

        Assert.That(request.Verb, Is.EqualTo(RequestVerb.Post));
        Assert.That(request.Url, Is.EqualTo("https://localhost:5167/todoitems"));
        Assert.That(request.HttpVersion, Is.Null);
        Assert.That(request.Headers, Is.Not.Empty);
        Assert.That(request.RequestBody, Is.EqualTo("""
            {
                "id": "id",
                "name":"walk dog",
                "isComplete":false
            }
            """));
        Assert.That(request.Variables, Is.Empty);
        Assert.That(request.Comment, Is.EqualTo("Create a new item"));
    }


    [Test]
    public async Task TestParse6()
    {
        byte[] data = Encoding.UTF8.GetBytes("""
            @base=https://localhost:5167

            ### Create a new item

            PUT {{base}}/todoitems
            Content-Type: application/json

            {
                "id": "{{$guid}}",
                "name":"walk dog",
                "isComplete":false
            }
            """);

        var sut = new HttpFileParser();

        var result = await sut.ParseAsync(new MemoryStream(data));

        Assert.That(result.Variables, Is.Not.Empty);

        Assert.That(result.Requests.Count, Is.EqualTo(1));

        var request = result.Requests[0];

        Assert.That(request.Verb, Is.EqualTo(RequestVerb.Put));
        Assert.That(request.Url, Is.EqualTo("{{base}}/todoitems"));
        Assert.That(request.HttpVersion, Is.Null);
        Assert.That(request.Headers, Is.Not.Empty);
        Assert.That(request.RequestBody, Is.EqualTo("""
            {
                "id": "{{$guid}}",
                "name":"walk dog",
                "isComplete":false
            }
            """));
        Assert.That(request.Variables, Is.Not.Empty);
        Assert.That(request.Comment, Is.EqualTo("Create a new item"));
    }
}
