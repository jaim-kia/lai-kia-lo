public interface IOfferingReceiver
{
    void OnAmountChosen(string incenseType, int amount, bool cancel);
}