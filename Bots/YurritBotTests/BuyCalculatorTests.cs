using FluentAssertions;
using NUnit.Framework;
using YurritBot;

namespace YurritBotTests;

[TestFixture]
public class BuyCalculatorTests
{
    [TestCase(500, 20, 25)]
    [TestCase(5000, 2, 1000)]
    public void CalculateMaximuumBuyAmountTest(decimal currentCash, decimal listingPrice, int expectedAmount)
    {
        // Arrange
        var buyCalculator = new BuyCalculator();

        // Act
        var maxAmount = buyCalculator.CalculateMaximuumBuyAmount(currentCash, listingPrice);

        // Assert
        maxAmount.Should().Be(expectedAmount);
    }
}