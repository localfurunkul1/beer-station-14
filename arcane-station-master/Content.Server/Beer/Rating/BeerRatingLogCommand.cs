using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Beer.Rating;

[AdminCommand(AdminFlags.Admin)]
public sealed class BeerRatingLogCommand : IConsoleCommand
{
    public string Command => "beer_rating_log";
    public string Description => "Показать последние записи рейтинга. Аргумент - имя игрока (опционально).";
    public string Help => "Usage: beer_rating_log [username]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var manager = IoCManager.Resolve<BeerRatingManager>();
        var filter = args.Length > 0 ? args[0] : null;
        var rows = manager.GetLog(filter, 50);

        if (rows.Count == 0)
        {
            shell.WriteLine("Нет записей.");
            return;
        }

        foreach (var (voter, target, tag, isPositive, createdAt) in rows)
        {
            var sign = isPositive ? "+" : "-";
            shell.WriteLine($"{createdAt:yyyy-MM-dd HH:mm} | {voter,-20} -> {target,-20} | {sign} \"{tag}\"");
        }
    }
}
