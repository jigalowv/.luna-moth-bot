namespace Luna.Application.Common.Dtos;

public record StructChange<T>(T Before, T? After) where T : struct;
public record ClassChange<T>(T Before, T? After) where T : class;