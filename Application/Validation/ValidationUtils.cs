using System.Text.RegularExpressions;

namespace Application.Validation
{
    public static class ValidationUtils
    {
        public static void ThrowIfNullOrEmpty(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Application.Exceptions.MyInvalidDataException($"{fieldName} no puede estar vacío.");
        }

        public static void ThrowIfOutOfRange(int value, int min, int max, string fieldName)
        {
            if (value < min || value > max)
                throw new Application.Exceptions.MyInvalidDataException($"{fieldName} debe estar entre {min} y {max}.");
        }

        public static void ThrowIfNegative(decimal value, string fieldName)
        {
            if (value < 0)
                throw new Application.Exceptions.MyInvalidDataException($"{fieldName} no puede ser negativo.");
        }

        public static void ThrowIfInvalidEmail(string? email, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Application.Exceptions.MyInvalidDataException($"{fieldName} es obligatorio.");
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email))
                throw new Application.Exceptions.MyInvalidDataException($"{fieldName} no tiene un formato válido.");
        }
    }
}
