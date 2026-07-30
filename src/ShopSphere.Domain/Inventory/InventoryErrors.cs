using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Inventory;

public static class InventoryErrors
{
    public static Error QuantityNotPositive(string operation) =>
        Error.Validation(
            "inventory.quantity_not_positive",
            $"{operation} quantity must be a positive integer.");

    public static Error InsufficientStock(int requested, int available) =>
        Error.Conflict(
            "inventory.insufficient_stock",
            $"Insufficient stock: requested {requested}, available {available}.");

    public static Error InsufficientReserved(int requested, int reserved) =>
        Error.Conflict(
            "inventory.insufficient_reserved",
            $"Cannot release {requested} units — only {reserved} reserved.");

    public static Error AdjustmentBelowZero(int currentAvailable, int delta) =>
        Error.Conflict(
            "inventory.adjustment_below_zero",
            $"Adjustment would drive Available below zero (was {currentAvailable}, delta {delta}).");
}