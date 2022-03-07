using System;
using System.Linq;
using ImGuiNET;

///
/// Provides an abstraction for asserting conditions.
///
public static class Assert
{
    private class AssertionException : Exception
    {
        public AssertionException(string message)
            : base(message)
        {
        }
    }

    private static void PrintStackTrace(string message)
    {
        ImGuiLog.Critical($"Assertion Failed\n {message}");
        var stackTrace = Environment.StackTrace.Split('\n');
        ImGuiLog.Critical(string.Join("\n", stackTrace.Skip(2)));
        // throw new AssertionException(message);
    }

    public static void Fail(string message = "")
    {
        PrintStackTrace("Fail");
        throw new AssertionException(message);
    }

    public static void Equals(object actual, object expected, string message = "")
    {
        if (actual.Equals(expected))
            return;

        PrintStackTrace($"Expected: {expected}\nActual: {actual}");
    }

    public static void True(bool value, string message = "")
    {
        if (value)
            return;

        PrintStackTrace($"Expected: true\nActual: false");
    }

    public static void False(bool value, string message = "")
    {
        if (!value)
            return;

        PrintStackTrace($"Expected: false\nActual: true");
    }

    public static void NotNull(object? value, string message = "")
    {
        if (value != null)
            return;

        PrintStackTrace($"Expected: {null} != null");
    }

    public static void Null(object? value, string message = "")
    {
        if (value == null)
            return;

        PrintStackTrace($"Expected: {value} == null");
    }

    public static void LessThan(float value, float lessThan, string message = "")
    {
        if (value < lessThan)
            return;

        PrintStackTrace($"Expected: {value} < {lessThan}");
    }

    public static void LessThanEquals(float value, float lessThanEquals, string message = "")
    {
        if (value <= lessThanEquals)
            return;

        PrintStackTrace($"Expected: {value} <= {lessThanEquals}");
    }

    public static void GreaterThan(float value, float greaterThan, string message = "")
    {
        if (value > greaterThan)
            return;

        PrintStackTrace($"Expected: {value} > {greaterThan}");
    }

    public static void GreaterThanEquals(float value, float greaterThanEquals, string message = "")
    {
        if (value >= greaterThanEquals)
            return;

        PrintStackTrace($"Expected: {value} >= {greaterThanEquals}");
    }

    public static void TupleLength(object tuple, int length, string message = "")
    {
        var tupleLength = tuple.GetType().GetGenericArguments().Length;
        if (tupleLength.Equals(length))
            return;
        
        PrintStackTrace($"Expected: {length}\nActual: {tupleLength}");
    }
}