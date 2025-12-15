using System;
using System.Globalization;
using System.Linq;
using System.Text;
using ACMS_Program_Planner.Models;

namespace ACMS_Program_Planner.Services;

/// <summary>
/// Encodes RFIDUnit program data to CSV format compatible with iOS ServiceCarouselViewModel parser.
/// CSV format: programNumber,numSteps,programName,stepId,stepName,durationSeconds,physicalUnit,targetValue,mode,termination1,termination2,...
/// </summary>
public static class ProgramCsvEncoder
{
    private const int MaxNameLength = 64;

    /// <summary>
    /// Converts an RFIDUnit to a CSV string for QR code encoding.
    /// </summary>
    /// <param name="rfidUnit">The RFID unit containing program data</param>
    /// <param name="error">Output error message if encoding fails</param>
    /// <returns>CSV string or null if encoding fails</returns>
    public static string? ToCsv(RFIDUnit rfidUnit, out string? error)
    {
        error = null;

        if (rfidUnit == null)
        {
            error = "RFID unit is null";
            return null;
        }

        if (rfidUnit.UnitProgram?.Unit == null)
        {
            error = "Program data is incomplete (missing unit)";
            return null;
        }

        if (rfidUnit.Program == null)
        {
            error = "No program found for RFID unit";
            return null;
        }

        if (rfidUnit.Program.StepIds.Count == 0)
        {
            error = "Program has no steps defined";
            return null;
        }

        var sb = new StringBuilder();

        // Header: programNumber, numSteps, programName
        int programNumber = rfidUnit.Program.Id;
        int numSteps = rfidUnit.Program.StepIds.Count;
        string programName = SanitizeName(rfidUnit.Program.Name);

        sb.Append(programNumber.ToString(CultureInfo.InvariantCulture));
        sb.Append(',');
        sb.Append(numSteps.ToString(CultureInfo.InvariantCulture));
        sb.Append(',');
        sb.Append(programName);

        // Steps: stepId, stepName, durationSeconds, physicalUnit, targetValue, mode, termination1, termination2
        foreach (var stepId in rfidUnit.Program.StepIds)
        {
            var step = DataService.Instance.Steps.FirstOrDefault(s => s.Id == stepId);
            if (step == null)
            {
                error = $"Step with ID {stepId} not found";
                return null;
            }

            sb.Append(',');
            sb.Append(step.Id.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(SanitizeName(step.Name));
            sb.Append(',');
            sb.Append(step.DurationSeconds.ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append("0"); // physicalUnit is always 0
            sb.Append(',');
            sb.Append(((int)step.FinalTemperature).ToString(CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(step.IsSteamerActive ? "1" : "0");
            sb.Append(',');
            sb.Append(step.TerminateIfTimeExpired ? "1" : "0");
            sb.Append(',');
            sb.Append(step.TerminateIfTemperatureReached ? "2" : "0");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Sanitizes a name for CSV by removing/replacing problematic characters.
    /// Commas are replaced with semicolons, newlines are removed, and length is capped.
    /// </summary>
    private static string SanitizeName(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        // Replace commas with semicolons (iOS parser splits on commas)
        var sanitized = name.Replace(',', ';');

        // Remove newlines and carriage returns
        sanitized = sanitized.Replace("\r", "").Replace("\n", "");

        // Trim whitespace
        sanitized = sanitized.Trim();

        // Cap length to keep QR code scannable
        if (sanitized.Length > MaxNameLength)
            sanitized = sanitized[..MaxNameLength];

        return sanitized;
    }
}
