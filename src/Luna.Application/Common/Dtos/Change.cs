namespace Luna.Application.Common.Dtos;

public record Change<T>(T Before, T After);