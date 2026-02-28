using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Petoria.Controllers;
using Petoria.Tests.Helpers;

namespace Petoria.Tests.Controllers;

public class StripeControllerTests : ControllerTestBase
{
    [Fact]
    public void GetConfig_ReturnsPublishableKey()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Stripe:PublishableKey", "pk_test_12345" },
                { "Stripe:SecretKey", "sk_test_12345" }
            })
            .Build();

        var controller = new StripeController(config);
        SetControllerUser(controller, "user1");

        var result = controller.GetConfig();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
