using Content.Server.Administration.Managers;
using Content.Shared.Beer.Rating;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Beer.Rating;

public sealed class BeerRatingSystem : EntitySystem
{
    [Dependency] private readonly BeerRatingManager _rating = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IAdminManager _admin = default!;

    private static readonly VerbCategory RatingCategory = new(
        "Социальный рейтинг",
        "/Textures/Interface/examine-star.png")
    {
        Columns = 2,
    };

    public override void Initialize()
    {
        base.Initialize();
        _rating.Initialize();

        SubscribeLocalEvent<ActorComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<ActorComponent, ExaminedEvent>(OnExamined);
    }

    private void OnGetVerbs(EntityUid uid, ActorComponent component, GetVerbsEvent<Verb> args)
    {
        if (args.User == uid)
            return;
        if (!TryComp<ActorComponent>(args.User, out var voterActor))
            return;

        var voterName = voterActor.PlayerSession.Name;
        var targetName = component.PlayerSession.Name;

        foreach (var (tag, isPositive) in BeerRatingTags.All)
        {
            var capturedTag = tag;
            var capturedPositive = isPositive;
            var verb = new Verb
            {
                Text = (isPositive ? "+ " : "- ") + tag,
                Category = RatingCategory,
                Priority = isPositive ? 1 : 0,
                Act = () => OnVote(args.User, voterName, targetName, capturedTag, capturedPositive),
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnVote(EntityUid voterEnt, string voterName, string targetName, string tag, bool isPositive)
    {
        if (!TryComp<ActorComponent>(voterEnt, out var voterActor))
            return;

        var isAdmin = _admin.IsAdmin(voterActor.PlayerSession);
        if (isAdmin)
        {
            _rating.CastVoteAdmin(voterName, targetName, tag, isPositive);
            var sign = isPositive ? "+" : "−";
            _popup.PopupEntity($"[Админ] Рейтинг {sign}1: \"{tag}\"", voterEnt, voterEnt, PopupType.Medium);
            return;
        }

        if (_rating.TryCastVote(voterName, targetName, tag, isPositive, out var cooldownLeft))
        {
            var sign = isPositive ? "+" : "−";
            _popup.PopupEntity($"Рейтинг {sign}1: \"{tag}\"", voterEnt, voterEnt, PopupType.Medium);
        }
        else
        {
            var minutes = (int)Math.Ceiling(cooldownLeft.TotalMinutes);
            _popup.PopupEntity($"Подождите ещё {minutes} мин до следующего голоса", voterEnt, voterEnt, PopupType.SmallCaution);
        }
    }

    private void OnExamined(EntityUid uid, ActorComponent component, ExaminedEvent args)
    {
        var username = component.PlayerSession.Name;
        var score = _rating.GetScore(username);
        var label = BeerRatingManager.ScoreLabel(score);
        var sign = score >= 0 ? "+" : "";
        var color = score switch
        {
            <= -10 => "#cc4040",
            <= -3 => "#dd9040",
            <= 10 => "#90cc60",
            _ => "#60ccc0",
        };
        var markup = new FormattedMessage();
        markup.AddMarkupOrThrow($"Репутация: [color={color}]{sign}{score} ({label})[/color]");
        args.PushMessage(markup);
    }
}
