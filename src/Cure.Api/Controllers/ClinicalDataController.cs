using Cure.Api.Extensions;
using Cure.Application.Abstractions;
using Cure.Application.DTOs.Clinical;
using Cure.Domain.Common;
using Cure.Domain.Entities;
using Cure.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cure.Api.Controllers;

[ApiController]
[Route("api/clinical")]
[Authorize]
public sealed class ClinicalDataController : ControllerBase
{
    private readonly IClinicalDataService _clinicalDataService;
    private readonly IUnitOfWork _unitOfWork;

    public ClinicalDataController(
        IClinicalDataService clinicalDataService,
        IUnitOfWork unitOfWork)
    {
        _clinicalDataService = clinicalDataService;
        _unitOfWork = unitOfWork;
    }

    // ──────────────────────────────────────────────
    //  Medical Histories
    // ──────────────────────────────────────────────

    [HttpGet("medical-histories/{id:guid}")]
    [ProducesResponseType(typeof(MedicalHistory), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicalHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.GetByIdAsync<MedicalHistory>(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("medical-histories")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(MedicalHistory), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMedicalHistory(
        [FromBody] MedicalHistoryDto dto,
        CancellationToken cancellationToken)
    {
        var entity = new MedicalHistory
        {
            Id = Guid.NewGuid(),
            PatientId = dto.PatientId,
            Condition = dto.Condition,
            DiagnosedDate = dto.DiagnosedDate,
            Treatment = dto.Treatment,
            IsCurrent = dto.IsCurrent
        };

        var result = await _clinicalDataService.CreateAsync(entity, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("medical-histories")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(MedicalHistory), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMedicalHistory(
        [FromBody] MedicalHistoryDto dto,
        CancellationToken cancellationToken)
    {
        var entity = new MedicalHistory
        {
            Id = dto.Id ?? Guid.Empty,
            PatientId = dto.PatientId,
            Condition = dto.Condition,
            DiagnosedDate = dto.DiagnosedDate,
            Treatment = dto.Treatment,
            IsCurrent = dto.IsCurrent,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var result = await _clinicalDataService.UpdateAsync(entity, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("medical-histories/{id:guid}")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedicalHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.DeleteAsync<MedicalHistory>(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("medical-histories/paged")]
    [ProducesResponseType(typeof(PagedResult<MedicalHistory>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMedicalHistoriesPaged(
        [FromQuery] PagedRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.GetPagedAsync<MedicalHistory>(
            null, request.Page, request.PageSize, cancellationToken);

        return result.ToActionResult();
    }

    // ──────────────────────────────────────────────
    //  Clinical Notes
    // ──────────────────────────────────────────────

    [HttpGet("clinical-notes/{id:guid}")]
    [ProducesResponseType(typeof(ClinicalNote), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClinicalNote(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.GetByIdAsync<ClinicalNote>(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("clinical-notes")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(ClinicalNote), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClinicalNote(
        [FromBody] ClinicalNoteDto dto,
        CancellationToken cancellationToken)
    {
        var entity = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = dto.PatientId,
            AuthorId = dto.AuthorId.ToString(),
            Content = dto.Content,
            NoteType = dto.NoteType
        };

        var result = await _clinicalDataService.CreateAsync(entity, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("clinical-notes")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(ClinicalNote), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClinicalNote(
        [FromBody] ClinicalNoteDto dto,
        CancellationToken cancellationToken)
    {
        var entity = new ClinicalNote
        {
            Id = dto.Id ?? Guid.Empty,
            PatientId = dto.PatientId,
            AuthorId = dto.AuthorId.ToString(),
            Content = dto.Content,
            NoteType = dto.NoteType,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var result = await _clinicalDataService.UpdateAsync(entity, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("clinical-notes/{id:guid}")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClinicalNote(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.DeleteAsync<ClinicalNote>(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("clinical-notes/paged")]
    [ProducesResponseType(typeof(PagedResult<ClinicalNote>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinicalNotesPaged(
        [FromQuery] PagedRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.GetPagedAsync<ClinicalNote>(
            null, request.Page, request.PageSize, cancellationToken);

        return result.ToActionResult();
    }

    // ──────────────────────────────────────────────
    //  Patient Files
    // ──────────────────────────────────────────────

    [HttpGet("patient-files/{id:guid}")]
    [ProducesResponseType(typeof(PatientFile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientFile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.GetByIdAsync<PatientFile>(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("patient-files")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(PatientFile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatientFile(
        [FromBody] PatientFileDto dto,
        CancellationToken cancellationToken)
    {
        var entity = new PatientFile
        {
            Id = Guid.NewGuid(),
            PatientId = dto.PatientId,
            FileName = dto.FileName,
            ContentType = dto.ContentType,
            FilePath = dto.FilePath,
            FileSize = dto.FileSize
        };

        var result = await _clinicalDataService.CreateAsync(entity, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("patient-files")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(PatientFile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatientFile(
        [FromBody] PatientFileDto dto,
        CancellationToken cancellationToken)
    {
        var entity = new PatientFile
        {
            Id = dto.Id ?? Guid.Empty,
            PatientId = dto.PatientId,
            FileName = dto.FileName,
            ContentType = dto.ContentType,
            FilePath = dto.FilePath,
            FileSize = dto.FileSize,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var result = await _clinicalDataService.UpdateAsync(entity, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("patient-files/{id:guid}")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatientFile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.DeleteAsync<PatientFile>(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("patient-files/paged")]
    [ProducesResponseType(typeof(PagedResult<PatientFile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientFilesPaged(
        [FromQuery] PagedRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _clinicalDataService.GetPagedAsync<PatientFile>(
            null, request.Page, request.PageSize, cancellationToken);

        return result.ToActionResult();
    }
}
