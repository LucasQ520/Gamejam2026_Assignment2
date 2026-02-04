public interface IInventoryProvider
{
    ItemId GetLeftHandItem();
    ItemId GetRightHandItem();

  
    ItemId GetSelectedItem();
}