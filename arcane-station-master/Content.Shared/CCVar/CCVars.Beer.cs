using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> BeerShadersEnabled =
        CVarDef.Create("beer.shaders_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerFisheyeEnabled =
        CVarDef.Create("beer.fisheye_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerSprintShakeEnabled =
        CVarDef.Create("beer.sprint_shake_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerWarmFilterEnabled =
        CVarDef.Create("beer.warm_filter_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerBloomEnabled =
        CVarDef.Create("beer.bloom_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerBulletFxEnabled =
        CVarDef.Create("beer.bullet_fx_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> BeerFisheyeIntensity =
        CVarDef.Create("beer.fisheye_intensity", 0.05f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> BeerSprintShakeIntensity =
        CVarDef.Create("beer.sprint_shake_intensity", 0.5f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> BeerWarmFilterIntensity =
        CVarDef.Create("beer.warm_filter_intensity", 0.04f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> BeerBloomIntensity =
        CVarDef.Create("beer.bloom_intensity", 0.55f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerBrightnessEnabled =
        CVarDef.Create("beer.brightness_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> BeerBrightnessIntensity =
        CVarDef.Create("beer.brightness_intensity", 0.07f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerVignetteEnabled =
        CVarDef.Create("beer.vignette_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> BeerBloodSplatterEnabled =
        CVarDef.Create("beer.blood_splatter_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

}
