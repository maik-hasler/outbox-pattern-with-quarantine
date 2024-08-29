namespace Api.Outbox.Quarantines;

public sealed class Quarantine
{
    public readonly static Quarantine Inactive = new();

    private readonly List<IQuarantine> _quarantines = [];

    public IReadOnlyCollection<IQuarantine> Quarantines => _quarantines;

    public bool IsActive() => _quarantines.Exists(q => q is ActiveQuarantine);

    public void Activate(DateTimeOffset enteredOnUtc) => _quarantines.Add(new ActiveQuarantine(enteredOnUtc));

    public void Deactivate(DateTimeOffset releasedOnUtc)
    {
        var latestEnteredQuarantineEntry = _quarantines.Find(q => q is ActiveQuarantine);

        if (latestEnteredQuarantineEntry is null)
        {
            throw new InvalidOperationException("No active quarantine found.");
        }

        // TODO: Does ef core create a new db row for this aswell? or does it update the old row?
        // Second is the needed behavior

        _quarantines.Remove(latestEnteredQuarantineEntry);

        _quarantines.Add(new FinishedQuarantine(latestEnteredQuarantineEntry.EnteredOnUtc, releasedOnUtc));
    }
}
