namespace StockManager.Contracts.Inventory
{
    public record OrderItem
    (
        Guid OrderId,
       Guid ProductId,
        int Quantity,
        decimal OrderPrice
       
    );
}