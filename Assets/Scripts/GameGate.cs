public static class GameGate
{
   public static bool GameplayEnabled { get; private set; } = false;

    public static void SetGameplayEnabled(bool enabled)
    {
        GameplayEnabled = enabled;
    }
}