using Dento.Data;
using Dento.Enums;
using Dento.Models;
using Microsoft.EntityFrameworkCore;

namespace Dento.Jobs;

public interface IReleaseLockedSlotJob
{
    Task ExecuteAsync(string slotId);
}

public class ReleaseLockedSlotJob : IReleaseLockedSlotJob
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReleaseLockedSlotJob> _logger;

    public ReleaseLockedSlotJob(AppDbContext context, ILogger<ReleaseLockedSlotJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync(string slotId)
    {
        _logger.LogInformation("Attempting to release lock from slot with ID: {SlotId}", slotId);

        var slot = await _context.Slots.SingleAsync(s => s.Id == slotId);

        if (slot.Status != SlotStatus.Locked)
            return;

        slot.Status = SlotStatus.Available;
        slot.LockedUntil = null;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is Slot)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues is null)
                    {
                        _logger.LogWarning(
                            "Concurrency conflict while releasing slot {SlotId}. The slot no longer exists.",
                            slotId);

                        continue;
                    }

                    var currentValues = entry.CurrentValues;
                    var originalValues = entry.OriginalValues;

                    _logger.LogWarning("""
                            Concurrency conflict while releasing slot {SlotId}.

                            Original Status : {OriginalStatus}
                            Current Status  : {CurrentStatus}
                            Database Status : {DatabaseStatus}

                            Original Version: {OriginalVersion}
                            Database Version: {DatabaseVersion}
                     """,
                        slotId,
                        originalValues["Status"],
                        currentValues["Status"],
                        databaseValues["Status"],
                        originalValues["RowVersion"],
                        databaseValues["RowVersion"]);
                }
            }
        }
    }
}
