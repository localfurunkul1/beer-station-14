using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Beer.Rating;

[AdminCommand(AdminFlags.Admin)]
public sealed class BeerRatingSetCommand : IConsoleCommand
{
    public string Command => "beer_rating_set";
    public string Description => "Установить абсолютное значение рейтинга для игрока.";
    public string Help => "Usage: beer_rating_set <username> <score>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Help);
            return;
        }
        if (!int.TryParse(args[1], out var score))
        {
            shell.WriteError("Score must be an integer.");
            return;
        }

        var manager = IoCManager.Resolve<BeerRatingManager>();
        manager.SetScore(args[0], score);
        shell.WriteLine($"Установлено: {args[0]} → {score}");
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class BeerRatingAdjustCommand : IConsoleCommand
{
    public string Command => "beer_rating_adjust";
    public string Description => "Изменить рейтинг игрока на дельту (+ или -).";
    public string Help => "Usage: beer_rating_adjust <username> <delta>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Help);
            return;
        }
        if (!int.TryParse(args[1], out var delta))
        {
            shell.WriteError("Delta must be an integer.");
            return;
        }

        var manager = IoCManager.Resolve<BeerRatingManager>();
        var newScore = manager.AdjustScore(args[0], delta);
        shell.WriteLine($"{args[0]}: {(delta >= 0 ? "+" : "")}{delta} → итог {newScore}");
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class BeerRatingGetCommand : IConsoleCommand
{
    public string Command => "beer_rating_get";
    public string Description => "Показать текущий рейтинг игрока.";
    public string Help => "Usage: beer_rating_get <username>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Help);
            return;
        }

        var manager = IoCManager.Resolve<BeerRatingManager>();
        var score = manager.GetScore(args[0]);
        var label = BeerRatingManager.ScoreLabel(score);
        shell.WriteLine($"{args[0]}: {(score >= 0 ? "+" : "")}{score} ({label})");
    }
}
