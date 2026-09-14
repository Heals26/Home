using Home.Application.UseCases.ShoppingLists.Models;
using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Infrastructure.ShoppingLists;

/// <summary>
/// What a price on a list means against what the household has paid before. Pure, so the sums that
/// decide "dearer than usual" are tested without a database.
/// </summary>
public static class ShoppingPriceLogic
{

    #region Fields

    /// <summary>
    /// How far over usual a price has to be before it is worth saying.
    /// </summary>
    public static readonly decimal DearerMargin = 0.10m;

    /// <summary>
    /// Grams and kilograms compare as kilograms, millilitres and litres as litres. Any other unit
    /// compares only with itself.
    /// </summary>
    private static readonly Dictionary<long, (string Basis, decimal Factor)> s_Scales = new()
    {
        [MeasurementUnitSE.Grams.Value] = ("kg", 0.001m),
        [MeasurementUnitSE.Kilograms.Value] = ("kg", 1m),
        [MeasurementUnitSE.Millilitres.Value] = ("L", 0.001m),
        [MeasurementUnitSE.Litres.Value] = ("L", 1m),
    };

    /// <summary>
    /// How many past purchases "usual" is taken from: enough to see past one special, few enough that
    /// a real rise becomes the usual within a month of weekly shops.
    /// </summary>
    public static readonly int UsualWindow = 5;

    #endregion Fields

    #region Methods

    public static ShoppingItemInsight Assess(ShoppingListItem item, ShoppingItemMemory? memory)
    {
        var _CategoryID = memory?.ShoppingCategory?.ShoppingCategoryID;
        var _Prices = memory?.Prices ?? [];

        // A line in the trolley is judged against the shops before this one, so only the purchase its
        // own tick recorded is left out. What the same line cost on earlier shops still counts, which
        // is all a list kept from week to week with Untick all has to go on.
        var _ThisShop = item.InBasket && item.Cost is > 0
            ? _Prices.Where(p => p.ShoppingListItemID == item.ShoppingListItemID).MaxBy(p => p.BoughtOnUTC)
            : null;

        var _History = _Prices
            .Where(p => p != _ThisShop)
            .OrderByDescending(p => p.BoughtOnUTC)
            .ToList();

        var _Line = Measure(item.Amount, item.Unit);

        var _Comparable = _History
            .Select(p => (Price: p, Measure: Measure(p.Amount, p.Unit)))
            .Where(p => p.Measure.Basis == _Line.Basis)
            .Take(UsualWindow)
            .Select(p => PerBasis(p.Price.Cost, p.Measure.Quantity))
            .ToList();

        var _Estimate = item.Cost == null ? Estimate(_Line, _History.FirstOrDefault()) : null;

        if (_Comparable.Count == 0)
            return new(_Estimate, false, _CategoryID, null);

        var _Usual = Median(_Comparable);
        var _IsDearer = item.Cost is { } _Cost && PerBasis(_Cost, _Line.Quantity) > _Usual * (1 + DearerMargin);

        return new(_Estimate, _IsDearer, _CategoryID, Round(_Line.Quantity is { } _Quantity ? _Usual * _Quantity : _Usual));
    }

    private static decimal? Estimate((string Basis, decimal? Quantity) line, ShoppingItemPrice? last)
    {
        if (last == null)
            return null;

        var _Last = Measure(last.Amount, last.Unit);

        // Scaled when both share a unit, so "1 kg" after "$7 for 2 kg" reads as $3.50. Otherwise the
        // last price as it was, which is still the best guess going.
        return Round(line.Basis == _Last.Basis && line.Quantity is { } _Quantity && _Last.Quantity is { } _LastQuantity
            ? last.Cost / _LastQuantity * _Quantity
            : last.Cost);
    }

    /// <summary>
    /// An amount on its comparable scale. A line with no amount is judged on its line price, and an
    /// amount with no unit counts in pieces, which is what "just a number" means on the list.
    /// </summary>
    private static (string Basis, decimal? Quantity) Measure(decimal? amount, long? unit)
    {
        if (amount is not { } _Amount || _Amount <= 0)
            return ("line", null);

        var _Unit = unit ?? MeasurementUnitSE.Pieces.Value;

        return s_Scales.TryGetValue(_Unit, out var _Scale)
            ? (_Scale.Basis, _Amount * _Scale.Factor)
            : ($"unit:{_Unit}", _Amount);
    }

    private static decimal Median(List<decimal> values)
    {
        var _Sorted = values.Order().ToList();
        var _Middle = _Sorted.Count / 2;

        return _Sorted.Count % 2 == 1
            ? _Sorted[_Middle]
            : (_Sorted[_Middle - 1] + _Sorted[_Middle]) / 2m;
    }

    private static decimal PerBasis(decimal cost, decimal? quantity)
        => quantity is { } _Quantity ? cost / _Quantity : cost;

    private static decimal Round(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    #endregion Methods

}
