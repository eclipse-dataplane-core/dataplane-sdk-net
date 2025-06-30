namespace Sdk.Core.Domain;

public class Criterion(object operandLeft, string @operator, object? operandRight = null)
{
    public object OperandLeft { get; } = operandLeft;
    public string Operator { get; } = @operator;
    public object? OperandRight { get; } = operandRight;
}