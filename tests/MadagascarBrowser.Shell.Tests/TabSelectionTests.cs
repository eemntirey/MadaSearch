using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Shell.Tests;

public class TabSelectionTests
{
    [Fact]
    public void NextIndex_FermetureDuDernierOnglet_ActiveLOngletPrecedent()
    {
        Assert.Equal(1, TabSelection.NextIndex(2, 3));
    }

    [Fact]
    public void NextIndex_FermetureDuPremierOnglet_ActiveLePremierRestant()
    {
        Assert.Equal(0, TabSelection.NextIndex(0, 2));
    }

    [Fact]
    public void NextIndex_FermetureDuPremierSurDeux_ActiveLePremierRestant()
    {
        Assert.Equal(0, TabSelection.NextIndex(0, 1));
    }

    [Fact]
    public void NextIndex_AucunOngletRestant_RetourneMoinsUn()
    {
        Assert.Equal(-1, TabSelection.NextIndex(0, 0));
    }

    [Fact]
    public void NextIndex_IndexHorsBornes_ResteDansLesLimites()
    {
        // Un index incohérent ne doit jamais désigner un onglet inexistant.
        var index = TabSelection.NextIndex(9, 2);
        Assert.InRange(index, 0, 1);
    }
}
