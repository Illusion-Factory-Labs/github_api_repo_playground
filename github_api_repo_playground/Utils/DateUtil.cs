using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace API_Conta_SaldoExtrato.Utils
{
    [ExcludeFromCodeCoverage]
    public static class DateUtil
  {
    /// <summary>
    /// Verifica se a string está no formato yyyy-MM-dd
    /// </summary>
    /// <param name="dateString">String da data a ser validada</param>
    /// <returns>True se estiver no formato correto, False caso contrário</returns>

    public static bool IsValidDateFormat(string dateString)
    {
      if (string.IsNullOrWhiteSpace(dateString))
        return false;

      return DateTime.TryParseExact(
          dateString,
          "yyyy-MM-dd",
          CultureInfo.InvariantCulture,
          DateTimeStyles.None,
          out _);
    }

    /// <summary>
    /// Verifica se a string está no formato yyyy-MM-dd e retorna a data convertida
    /// </summary>
    /// <param name="dateString">String da data a ser validada</param>
    /// <param name="date">Data convertida se válida</param>
    /// <returns>True se estiver no formato correto, False caso contrário</returns>
    public static bool TryParseIsoDate(string dateString, out DateTime date)
    {
      return DateTime.TryParseExact(
          dateString,
          "yyyy-MM-dd",
          CultureInfo.InvariantCulture,
          DateTimeStyles.None,
          out date);
    }

    /// <summary>
    /// Converte DateTime para string no formato yyyy-MM-dd
    /// </summary>
    /// <param name="date">Data a ser convertida</param>
    /// <returns>String no formato yyyy-MM-dd</returns>        
    public static string ToIsoDateString(this DateTime date)
    {
      return date.ToString("yyyy-MM-dd");
    }
  }
}