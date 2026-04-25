using System.Formats.Tar;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Contracts.Dtos.Repetition;
using Mnemo.Contracts.Dtos.Vocabulary;
using Mnemo.Contracts.Dtos.Vocabulary.Requests;
using Mnemo.Data.Entities;

namespace Mnemo.Common
{
    public static class Mapper
    {
        public static bool ValidDto(CreateVocabularyEntryRequest? dto)
        {
            return dto != null && dto.Foreign != string.Empty && dto.Translations != null && dto.Translations.Any(t => !string.IsNullOrWhiteSpace(t));
        }

        public static bool ValidDto(PatchVocabularyEntryRequest? dto)
        {
            if (dto == null) return false;
            var properties = dto.GetType().GetProperties();
            return properties.Any(p => p.GetValue(dto) != null);
        }


        public static VocabularyEntryResponse? MapToDto(VocabularyEntry? entry)
        {
            if (entry == null) return null;

            return new VocabularyEntryResponse
            {
                Id              =   entry.Id,
                Foreign         =   PrepareForeign(entry.Foreign),
                Transcription   =   PrepareTranscription(entry.Transcription),
                Examples        =   PrepareExamples(entry.Examples),
                Translations    =   PrepareTranslations(entry.Translations)
            };
        }

        public static VocabularyEntryResponse[] MapToDto(IEnumerable<VocabularyEntry> entries)
        {
            return entries
                .Where(e => e != null)
                .Select(e => MapToDto(e)!)
                .Distinct()
                .ToArray();
        }

        public static RepetitionResultResponse? MapToDto(RepetitionResult? result)
        {
            if (result == null) return null;

            return new RepetitionResultResponse
            {
                Correct = result.Correct,
                Total   = result.Total,
                Percent = result.Percent,
                StartedAt = result.StartedAt,
                FinishedAt = result.FinishedAt,
                VocabularyEntries = MapToDto(result.VocabularyEntries)
            };
        }

        public static RepetitionTaskResponse? MapToDto(RepetitionTask? task)
        {
            if (task == null) return null;

            return new RepetitionTaskResponse
            {
                Id          =   task.Id,
                Prompt      =   task.Prompt,
                UserAnswer  =   task.UserAnswer
            };
        }

        public static RepetitionTaskResponse[] MapToDto(IEnumerable<RepetitionTask> task)
        {
            return task
                .Where(e => e != null)
                .Select(e => MapToDto(e)!)
                .Distinct()
                .ToArray();
        }

        public static RepetitionStateResponse? MapToDto(RepetitionState? state)
        {
            if (state == null) return null;

            return new RepetitionStateResponse
            {
                Id                  = state.Id,
                RepetitionCounter   = state.RepetitionCounter,
                RepetitionInterval  = state.RepetitionInterval,
                EasinessFactor      = state.EasinessFactor,
                CanSelfAssess       = state.CanSelfAssess,
                NextRepetitionAt    = state.NextRepetitionAt,
                VocabularyEntry     = MapToDto(state.VocabularyEntry)
            };
        }

        public static RepetitionStateResponse[] MapToDto(IEnumerable<RepetitionState> states)
        {
            return states
                .Where(e => e != null)
                .Select(e => MapToDto(e)!)
                .Distinct()
                .ToArray();
        }


        public static VocabularyEntry MapToEntry(CreateVocabularyEntryRequest dto, int userId)
        {
            return new VocabularyEntry()
            {
                Foreign         =   PrepareForeign(dto.Foreign),
                Transcription   =   PrepareTranscription(dto.Transcription),
                Examples        =   PrepareExamples(dto.Examples).ToList(),
                Translations    =   PrepareTranslations(dto.Translations).ToList(),
                UserId          =   userId
            };
        }


        public static void PatchFromDto(VocabularyEntry entry, PatchVocabularyEntryRequest patchDto)
        {
            // Foreign patch
            if (!string.IsNullOrWhiteSpace(patchDto.Foreign))
                entry.Foreign = PrepareForeign(patchDto.Foreign);

            // Transcription patch
            if (!string.IsNullOrWhiteSpace(patchDto.Transcription))
                entry.Transcription = PrepareTranscription(patchDto.Transcription);

            // Examples add
            if (patchDto.ExamplesAdd != null)
            {
                var newExamples = new HashSet<string>(PrepareExamples(patchDto.ExamplesAdd)
                    .Where(e => !string.IsNullOrWhiteSpace(e)));
                entry.Examples.AddRange(newExamples);
            }

            // Examples remove
            if (patchDto.ExamplesRemove != null)
            {
                var toRemove = new HashSet<string>(PrepareExamples(patchDto.ExamplesRemove)
                    .Where(e => !string.IsNullOrWhiteSpace(e)));
                entry.Examples.RemoveAll(toRemove.Contains);
            }

            // Translations add
            if (patchDto.TranslationsAdd != null)
            {
                var newTranslations = new HashSet<string>(PrepareTranslations(patchDto.TranslationsAdd)
                    .Where(e => !string.IsNullOrWhiteSpace(e)));
                entry.Translations.AddRange(newTranslations);
            }

            // Translations remove
            if (patchDto.TranslationsRemove != null)
            {
                var translationsToRemove = new HashSet<string>(PrepareTranslations(patchDto.TranslationsRemove)
                    .Where(e => !string.IsNullOrWhiteSpace(e)));
                entry.Translations.RemoveAll(translationsToRemove.Contains);
            }
        }


        public static string PrepareForeign(string? foreign)
        {
            if (foreign == null)
                return string.Empty;


            return foreign
                .RemoveMultispaces()
                .ToLowerInvariant();
        }

        public static string PrepareTranscription(string? trascription)
        {
            if (trascription == null)
                return string.Empty;


            return trascription
                .RemoveMultispaces()
                .ToLowerInvariant()
                .WrapWithBracketsIfNeeded();
        }

        public static string[] PrepareExamples(IEnumerable<string>? examples)
        {
            if (examples == null)
                return Array.Empty<string>();

            return examples
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.RemoveMultispaces()
                               .ToLowerInvariant()
                               .AddLastPointIfNeeded())
                .Distinct()
                .ToArray();
        }

        public static string[] PrepareTranslations(IEnumerable<string>? translations)
        {
            if (translations == null)
                return Array.Empty<string>();

            return translations
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.RemoveMultispaces()
                               .ToLowerInvariant())
                .Distinct()
                .ToArray();
        }
    }
}
