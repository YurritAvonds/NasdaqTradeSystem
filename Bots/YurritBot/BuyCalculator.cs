using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YurritBot;

public class BuyCalculator
{
    private const int maximumBuyAmountPerStock = 1000;

    public int CalculateMaximuumBuyAmount(decimal currentCash, decimal listingPrice)
    {
        decimal maximumBuyAmountWithCurrentCash = currentCash / listingPrice;
        return (int)Math.Floor(Math.Min(maximumBuyAmountPerStock, maximumBuyAmountWithCurrentCash));
    }
}
