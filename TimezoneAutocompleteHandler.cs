

using Discord;
using Discord.Interactions;

public class TimezoneAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var partialStr = (string)autocompleteInteraction.Data.Current.Value;

        List<AutocompleteResult> results = new();

        foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
        {
            if (!TimeZoneInfo.TryConvertWindowsIdToIanaId(tz.Id, out var ianaId))
                continue;

            if (ianaId.ToLower().Contains(partialStr) || tz.DisplayName.ToLower().Contains(partialStr))
            {
                results.Add(new AutocompleteResult($"{tz.DisplayName} ({ianaId})", ianaId));
            }
        }

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
