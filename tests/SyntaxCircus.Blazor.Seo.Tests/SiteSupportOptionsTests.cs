namespace SyntaxCircus.Blazor.Seo.Tests;

public class SiteSupportOptionsTests
{
    [Fact]
    public void Defaults_AreAllNull()
    {
        var options = new SiteSupportOptions();

        options.Email.ShouldBeNull();
        options.IssueTrackerUrl.ShouldBeNull();
        options.IssueTemplateUrl.ShouldBeNull();
        options.OwnerContactUrl.ShouldBeNull();
        options.OwnerDisplayName.ShouldBeNull();
    }

    [Fact]
    public void SectionName_IsSupport()
    {
        SiteSupportOptions.SectionName.ShouldBe("Support");
    }
}
