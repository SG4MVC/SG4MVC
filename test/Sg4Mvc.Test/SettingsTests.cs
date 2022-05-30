using Sg4Mvc.Generator;
using Xunit;

namespace Sg4Mvc.Test;

public class SettingsTests
{
    [Fact]
    public void HelpersPrefix_Default() => Assert.NotNull(new Settings().HelpersPrefix);

    [Fact]
    public void Sg4MvcNamespace_Default() => Assert.NotNull(new Settings().Sg4MvcNamespace);

    [Fact]
    public void Linksnamespace_Default() => Assert.NotNull(new Settings().LinksNamespace);
}
