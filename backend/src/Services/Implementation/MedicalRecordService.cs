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
            .Include(m => m.VisitRecords)
                .ThenInclude(v => v.Prescriptions)
            .Include(m => m.VisitRecords)
                .ThenInclude(v => v.Procedures)
            .FirstOrDefaultAsync(m => m.PatientId == patientId);

        if (record == null)
            return null;

        return new MedicalRecordDto
        {
            Id = record.Id,
            PatientId = record.PatientId,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
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
}
