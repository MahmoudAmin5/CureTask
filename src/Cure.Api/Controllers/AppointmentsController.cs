using Cure.Api.Extensions;
using Cure.Application.DTOs.Appointment;
using Cure.Domain.Common;
using Cure.Domain.Entities;
using Cure.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost("book")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Book(
        [FromBody] BookAppointmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.BookAppointmentAsync(
            dto.PatientId,
            dto.NurseId,
            dto.ScheduledAtUtc,
            dto.DurationMinutes,
            dto.Location,
            dto.Notes,
            cancellationToken);

        if (result.IsSuccess)
        {
            var responseDto = AppointmentResponseDto.Map(result.Value);
            return Ok(responseDto);
        }

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetByIdAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            var responseDto = AppointmentResponseDto.Map(result.Value);
            return Ok(responseDto);
        }

        return result.ToActionResult();
    }

    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatientId(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetByPatientIdAsync(patientId, cancellationToken);

        if (result.IsSuccess)
        {
            var responseDtos = result.Value.Select(AppointmentResponseDto.Map).ToList();
            return Ok(responseDtos);
        }

        return result.ToActionResult();
    }

    [HttpGet("nurse/{nurseId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNurseId(
        Guid nurseId,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetByNurseIdAsync(nurseId, cancellationToken);

        if (result.IsSuccess)
        {
            var responseDtos = result.Value.Select(AppointmentResponseDto.Map).ToList();
            return Ok(responseDtos);
        }

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}/cancel")]
    [Authorize(Policy = "NurseOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CancelAppointmentAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResult<AppointmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _appointmentService.GetPagedAsync(page, pageSize, cancellationToken);

        if (result.IsSuccess)
        {
            var pagedResult = result.Value;
            var mappedItems = pagedResult.Items.Select(AppointmentResponseDto.Map).ToList();
            var response = new PagedResult<AppointmentResponseDto>(
                mappedItems, pagedResult.TotalCount, pagedResult.Page, pagedResult.PageSize);
            return Ok(response);
        }

        return result.ToActionResult();
    }
}
