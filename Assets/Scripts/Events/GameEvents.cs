using System;

public static class GameEvents
{
      public static Action StartGame;
}

//USAGE
public class Test
{
    Test()
    {
        GameEvents.StartGame?.Invoke();
    }
}
