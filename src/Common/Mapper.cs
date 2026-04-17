using System.Formats.Tar;
using System.Reflection;
using Itereta.Contracts.Dtos.Iteration;
using Itereta.Contracts.Dtos.Vocabulary;
using Itereta.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Itereta.Common
{
    public static class Mapper
    {
        public static bool ValidDto(VocabularyCreateDto? dto)
        {
            return dto != null && dto.Foreign != string.Empty && dto.Translations != null && dto.Translations.Any(t => !string.IsNullOrWhiteSpace(t));
        }

        public static bool ValidDto(VocabularyPatchDto? dto)
        {
            if (dto == null) return false;
            var properties = dto.GetType().GetProperties();
            return properties.Any(p => p.GetValue(dto) != null);
        }


        public static VocabularyResponseDto? MapToDto(VocabularyEntry? entry)
        {
            if (entry == null) return null;

            return new VocabularyResponseDto
            {
                Id              =   entry.Id,
                Foreign         =   PrepareForeign(entry.Foreign),
                Transcription   =   PrepareTranscription(entry.Transcription),
                Examples        =   PrepareExamples(entry.Examples),
                Translations    =   PrepareTranslations(entry.Translations)
            };
        }

        public static VocabularyResponseDto[] MapToDto(IEnumerable<VocabularyEntry> entries)
        {
            return entries
                .Where(e => e != null)
                .Select(e => MapToDto(e)!)
                .Distinct()
                .ToArray();
        }

        public static IteretteResponseDto? MapToDto(Iterette? iterette)
        {
            if (iterette == null) return null;

            return new IteretteResponseDto
            {
                Id          =   iterette.Id,
                Prompt      =   iterette.Prompt,
                UserAnswer  =   iterette.UserAnswer
            };
        }

        public static IteretteResponseDto[] MapToDto(IEnumerable<Iterette> iterettes)
        {
            return iterettes
                .Where(e => e != null)
                .Select(e => MapToDto(e)!)
                .Distinct()
                .ToArray();
        }


        public static VocabularyEntry MapToEntry(VocabularyCreateDto dto, User user)
        {
            return new VocabularyEntry()
            {
                User            =   user, 
                Foreign         =   PrepareForeign(dto.Foreign),
                Transcription   =   PrepareTranscription(dto.Transcription),
                Examples        =   PrepareExamples(dto.Examples).ToList(),
                Translations    =   PrepareTranslations(dto.Translations).ToList()
            };
        }


        public static void PatchFromDto(VocabularyEntry entry, VocabularyPatchDto patchDto)
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
