using MadagascarBrowser.Core.Security;

namespace MadagascarBrowser.Tests;

public sealed class PrivateBrowsingPolicyTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ShouldPersistHistory_RespectsPrivateMode(bool isPrivate, bool expected)
    {
        Assert.Equal(expected, PrivateBrowsingPolicy.ShouldPersistHistory(isPrivate, historyWorthy: true));
    }

    [Fact]
    public void ShouldPersistHistory_RejectsNonHistoryPagesEvenOutsidePrivateMode()
    {
        Assert.False(PrivateBrowsingPolicy.ShouldPersistHistory(isPrivate: false, historyWorthy: false));
        Assert.False(PrivateBrowsingPolicy.ShouldPersistHistory(isPrivate: true, historyWorthy: false));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void PrivateActivityIsNotPersisted(bool isPrivate, bool expected)
    {
        Assert.Equal(expected, PrivateBrowsingPolicy.ShouldPersistDownloadMetadata(isPrivate));
        Assert.Equal(expected, PrivateBrowsingPolicy.ShouldPersistFavorites(isPrivate));
        Assert.Equal(expected, PrivateBrowsingPolicy.ShouldPersistOfflinePages(isPrivate));
        Assert.Equal(expected, PrivateBrowsingPolicy.ShouldPersistSession(isPrivate));
    }
}
