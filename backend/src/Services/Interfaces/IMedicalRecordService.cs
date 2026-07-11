using Dento.DTOs;

namespace Dento.Services.Interfaces;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto?> GetMedicalRecordByPatientIdAsync(string patientId);
    Task<VisitMedicalRecordDto> AddVisitRecordAsync(string patientId, CreateVisitRecordDto dto);
}
