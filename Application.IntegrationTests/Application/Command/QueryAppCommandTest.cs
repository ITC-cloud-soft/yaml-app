using FluentAssertions;
using NUnit.Framework;
using Yaml.Application.Query;
using static YamlTest.Testing;
namespace YamlTest.Application.Command;

public class QueryAppCommandTest: BaseTestFixture
{


    [Test]
    public async Task Should_Get_AppCommand()
    {
        // arrange
        GetAppQuery query = new GetAppQuery()
        {
            AppId = 1,
            UserId = 1
        };
        // execute
        var result = await SendAsync(query);
        
        // assert
        result.Should().Be(1);
    }
}