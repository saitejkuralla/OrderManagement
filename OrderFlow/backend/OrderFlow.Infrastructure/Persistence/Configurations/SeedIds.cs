namespace OrderFlow.Infrastructure.Persistence.Configurations;

/// <summary>Deterministic ids for seed data so demo data is stable across database resets.</summary>
internal static class SeedIds
{
    public static readonly Guid AliceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BobId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CharlieId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DavidId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static readonly Guid LaptopId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid MonitorId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid KeyboardId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid MouseId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
}
