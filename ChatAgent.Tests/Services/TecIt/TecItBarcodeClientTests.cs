using ChatAgent.Models;
using ChatAgent.Services.TecIt;

namespace ChatAgent.Tests.Services.TecIt;

public class TecItBarcodeClientTests
{
    [Fact]
    public void BuildQueryString_WithEan13_IncludesExpectedParameters()
    {
        var label = new LabelData { BarcodeType = BarcodeType.Ean13, BarcodeData = "4006381333931" };

        var query = TecItBarcodeClient.BuildQueryString(label, "test-access-id");

        Assert.Equal("accessid=test-access-id&code=EAN13&data=4006381333931&imagetype=png", query);
    }

    [Fact]
    public void BuildQueryString_WithCode128_UsesCode128Identifier()
    {
        var label = new LabelData { BarcodeType = BarcodeType.Code128, BarcodeData = "ABC-123" };

        var query = TecItBarcodeClient.BuildQueryString(label, "test-access-id");

        Assert.Contains("code=Code128", query);
    }

    [Fact]
    public void BuildQueryString_WithNullBarcodeType_DefaultsToEan13()
    {
        var label = new LabelData { BarcodeType = null, BarcodeData = "4006381333931" };

        var query = TecItBarcodeClient.BuildQueryString(label, "test-access-id");

        Assert.Contains("code=EAN13", query);
    }

    [Fact]
    public void BuildQueryString_UrlEncodesSpecialCharactersInData()
    {
        var label = new LabelData { BarcodeType = BarcodeType.Code128, BarcodeData = "AB C&D" };

        var query = TecItBarcodeClient.BuildQueryString(label, "test-access-id");

        Assert.Contains("data=AB%20C%26D", query);
    }

    [Fact]
    public void BuildQueryString_WithNullBarcodeData_UsesEmptyDataValue()
    {
        var label = new LabelData { BarcodeType = BarcodeType.Code128, BarcodeData = null };

        var query = TecItBarcodeClient.BuildQueryString(label, "test-access-id");

        Assert.Contains("data=&", query);
    }
}
