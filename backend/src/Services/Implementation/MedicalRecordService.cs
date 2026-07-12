using Dento.Data;
using Dento.DTOs;
using Dento.Exceptions;
using Dento.Models;
using Dento.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dento.Services.Implementation;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly AppDbContext _context;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(AppDbContext context, ILogger<MedicalRecordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MedicalRecordDto?> GetMedicalRecordByPatientIdAsync(string patientId)
    {
        var record = await _context.MedicalRecords
            .Include(m => m.MedicalHistory)
            .Include(m => m.VisitRecords)
                .ThenInclude(v => v.Prescriptions)
            .Include(m => m.VisitRecords)
                .ThenInclude(v => v.Procedures)
            .FirstOrDefaultAsync(m => m.PatientId == patientId);

        if (record == null)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
            if (!patientExists)
                return null;

            record = new MedicalRecord
            {
                PatientId = patientId,
                MedicalHistory = new MedicalHistory()
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();
        }
        else if (record.MedicalHistory == null)
        {
            record.MedicalHistory = new MedicalHistory();
            await _context.SaveChangesAsync();
        }

        return new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = record.PatientId,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            MedicalHistory = new MedicalHistoryDto
            {
                Id = record.MedicalHistory.Id,
                MedicalRecordId = record.MedicalHistory.MedicalRecordId,
                MedicalConditions = record.MedicalHistory.MedicalConditions,
                Allergies = record.MedicalHistory.Allergies,
                PregnancyStatus = record.MedicalHistory.PregnancyStatus,
                SmokingStatus = record.MedicalHistory.SmokingStatus,
                BleedingDisorders = record.MedicalHistory.BleedingDisorders,
                HeartConditions = record.MedicalHistory.HeartConditions,
                Diabetes = record.MedicalHistory.Diabetes,
                HighBloodPressure = record.MedicalHistory.HighBloodPressure,
                MedicalNotes = record.MedicalHistory.MedicalNotes
            },
            VisitRecords = record.VisitRecords.Select(v => new VisitMedicalRecordDto
            {
                Id = v.Id,
                AppointmentId = v.AppointmentId,
                Diagnosis = v.Diagnosis,
                CreatedAt = v.CreatedAt,
                Prescriptions = v.Prescriptions.Select(p => new PrescriptionDto
                {
                    Id = p.Id,
                    MedicationName = p.MedicationName,
                    Dosage = p.Dosage,
                    Notes = p.Notes
                }).ToList(),
                Procedures = v.Procedures.Select(p => new ProcedureDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                }).ToList()
            }).ToList()
        };
    }

    public async Task<VisitMedicalRecordDto> AddVisitRecordAsync(string patientId, CreateVisitRecordDto dto)
    {
        var medicalRecord = await _context.MedicalRecords
            .FirstOrDefaultAsync(m => m.PatientId == patientId);

        if (medicalRecord == null)
        {
            _logger.LogWarning("Medical record not found for patient {PatientId}", patientId);
            throw new BaseException(StatusCodes.Status404NotFound, "Medical record not found for the specified patient.");
        }

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId && a.PatientId == patientId);

        if (appointment == null)
        {
            _logger.LogWarning("Appointment not found or does not belong to patient {PatientId}", patientId);
            throw new BaseException(StatusCodes.Status404NotFound, "Appointment not found or does not belong to the patient.");
        }

        var existingVisitRecord = await _context.VisitMedicalRecords
            .AnyAsync(v => v.AppointmentId == dto.AppointmentId);

        if (existingVisitRecord)
        {
            throw new BaseException(StatusCodes.Status400BadRequest, "A visit record already exists for this appointment.");
        }

        var visitRecord = new VisitMedicalRecord
        {
            MedicalRecordId = medicalRecord.Id,
            AppointmentId = dto.AppointmentId,
            Diagnosis = dto.Diagnosis,
            Prescriptions = dto.Prescriptions.Select(p => new Prescription
            {
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Notes = p.Notes
            }).ToList(),
            Procedures = dto.Procedures.Select(p => new Procedure
            {
                Name = p.Name,
                Description = p.Description
            }).ToList()
        };

        medicalRecord.UpdatedAt = DateTime.UtcNow;

        _context.VisitMedicalRecords.Add(visitRecord);
        await _context.SaveChangesAsync();

        return new VisitMedicalRecordDto
        {
            Id = visitRecord.Id,
            AppointmentId = visitRecord.AppointmentId,
            Diagnosis = visitRecord.Diagnosis,
            CreatedAt = visitRecord.CreatedAt,
            Prescriptions = visitRecord.Prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Notes = p.Notes
            }).ToList(),
            Procedures = visitRecord.Procedures.Select(p => new ProcedureDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            }).ToList()
        };
    }

    public async Task<VisitMedicalRecordDto> UpdateVisitRecordAsync(string patientId, string visitRecordId, UpdateVisitRecordDto dto)
    {
        var visitRecord = await _context.VisitMedicalRecords
            .Include(v => v.Prescriptions)
            .Include(v => v.Procedures)
            .Include(v => v.MedicalRecord)
            .FirstOrDefaultAsync(v => v.Id == visitRecordId && v.MedicalRecord.PatientId == patientId);

        if (visitRecord == null)
        {
            _logger.LogWarning("Visit record {VisitRecordId} not found for patient {PatientId}", visitRecordId, patientId);
            throw new BaseException(StatusCodes.Status404NotFound, "Visit record not found.");
        }

        visitRecord.Diagnosis = dto.Diagnosis;

        visitRecord.Prescriptions.Clear();
        foreach (var p in dto.Prescriptions)
        {
            visitRecord.Prescriptions.Add(new Prescription
            {
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Notes = p.Notes
            });
        }

        visitRecord.Procedures.Clear();
        foreach (var p in dto.Procedures)
        {
            visitRecord.Procedures.Add(new Procedure
            {
                Name = p.Name,
                Description = p.Description
            });
        }

        visitRecord.MedicalRecord.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new VisitMedicalRecordDto
        {
            Id = visitRecord.Id,
            AppointmentId = visitRecord.AppointmentId,
            Diagnosis = visitRecord.Diagnosis,
            CreatedAt = visitRecord.CreatedAt,
            Prescriptions = visitRecord.Prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Notes = p.Notes
            }).ToList(),
            Procedures = visitRecord.Procedures.Select(p => new ProcedureDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            }).ToList()
        };
    }

    public async Task<MedicalHistoryDto> UpdateMedicalHistoryAsync(string patientId, UpdateMedicalHistoryDto dto)
    {
        var history = await _context.MedicalHistories
            .Include(h => h.MedicalRecord)
            .FirstOrDefaultAsync(h => h.MedicalRecord.PatientId == patientId);

        if (history == null)
        {
            _logger.LogWarning("Medical history not found for patient {PatientId}", patientId);
            throw new BaseException(StatusCodes.Status404NotFound, "Medical history not found.");
        }

        history.MedicalConditions = dto.MedicalConditions;
        history.Allergies = dto.Allergies;
        history.PregnancyStatus = dto.PregnancyStatus;
        history.SmokingStatus = dto.SmokingStatus;
        history.BleedingDisorders = dto.BleedingDisorders;
        history.HeartConditions = dto.HeartConditions;
        history.Diabetes = dto.Diabetes;
        history.HighBloodPressure = dto.HighBloodPressure;
        history.MedicalNotes = dto.MedicalNotes;

        history.MedicalRecord.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new MedicalHistoryDto
        {
            Id = history.Id,
            MedicalRecordId = history.MedicalRecordId,
            MedicalConditions = history.MedicalConditions,
            Allergies = history.Allergies,
            PregnancyStatus = history.PregnancyStatus,
            SmokingStatus = history.SmokingStatus,
            BleedingDisorders = history.BleedingDisorders,
            HeartConditions = history.HeartConditions,
            Diabetes = history.Diabetes,
            HighBloodPressure = history.HighBloodPressure,
            MedicalNotes = history.MedicalNotes
        };
    }
}
