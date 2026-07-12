using Dento.DTOs;

namespace Dento.Services.Interfaces;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto?> GetMedicalRecordByPatientIdAsync(string patientId);
    Task<VisitMedicalRecordDto> AddVisitRecordAsync(string patientId, CreateVisitRecordDto dto);
    Task<VisitMedicalRecordDto> UpdateVisitRecordAsync(string patientId, string visitRecordId, UpdateVisitRecordDto dto);
    Task<MedicalHistoryDto> UpdateMedicalHistoryAsync(string patientId, UpdateMedicalHistoryDto dto);
}
