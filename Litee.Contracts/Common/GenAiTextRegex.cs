using System.Text.RegularExpressions;

namespace Litee.Contracts.Common;

public partial class GenAiTextRegex
{
  [GeneratedRegex(@"```json\s*(\{[\s\S]*?\})\s*```")]
  public static partial Regex JsonBlockRegex();

}
