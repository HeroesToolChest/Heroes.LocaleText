using System.Globalization;

namespace Heroes.LocaleText.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var enus = StormLocaleData.GetCultureInfo(StormLocale.ENUS);
        var dede = StormLocaleData.GetCultureInfo(StormLocale.DEDE);

        string asa = "0.04";
        string asa2 = "0,04";

        double org = double.Parse(asa, CultureInfo.InvariantCulture);
        double org2 = double.Parse(asa2, CultureInfo.InvariantCulture);
        //double testdede = double.Parse(asa, dede);
        //double testdede = double.Parse(asa, dede);
    }
}