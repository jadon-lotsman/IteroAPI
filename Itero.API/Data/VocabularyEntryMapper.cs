using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itero.API.Data.Entities;
using Itero.API.Dtos;

namespace Itero.API.Data
{
    public class VocabularyEntryMapper
    {
        public VocabularyEntryMapper(User user)
        {
            _entry = new VocabularyEntry();
            _entry.User = user;
        }

        private VocabularyEntry _entry;


        public VocabularyEntry Map(VocabularyEntryDTO entryDTO)
        {
            string foreign = PrepareForeign(entryDTO.Foreign);
            string trascritpion = PrepareTranscription(entryDTO.Transcription);
            var examples = PrepareExamples(entryDTO.Examples);
            var translations = PrepareTranslations(entryDTO.Translations);


            _entry.Foreign = foreign;
            _entry.Transcription = trascritpion;
            _entry.Examples = examples;
            _entry.Translations = translations;

            return _entry;
        }

        public VocabularyEntry MapPatched(VocabularyPatchDTO patchDTO, VocabularyEntry based)
        {
            _entry = based;

            // Foreign patch
            if (patchDTO.Foreign != null)
                _entry.Foreign = PrepareForeign(patchDTO.Foreign);


            // Transcription patch
            if (patchDTO.Transcription != null)
                _entry.Transcription = PrepareTranscription(patchDTO.Transcription);


            // Examples add
            if (patchDTO.ExamplesAdd != null)
                _entry.Examples.AddRange(PrepareExamples(patchDTO.ExamplesAdd));


            // Examples remove
            if (patchDTO.ExamplesRemove != null)
            {
                var examplesToRemove = new HashSet<string>(PrepareExamples(patchDTO.ExamplesRemove));
                _entry.Examples.RemoveAll(examplesToRemove.Contains);
            }


            // Translations add
            if (patchDTO.TranslationsAdd != null)
                _entry.Translations.AddRange(PrepareTranslations(patchDTO.TranslationsAdd));


            // Translations remove
            if (patchDTO.TranslationsRemove != null)
            {
                var translationsToRemove = new HashSet<string>(PrepareTranslations(patchDTO.TranslationsRemove));
                _entry.Translations.RemoveAll(translationsToRemove.Contains);
            }


            return _entry;
        }


        public string PrepareForeign(string foreign)
        {
            return foreign.RemoveMultispaces()
                .ToLowerInvariant();
        }

        public string PrepareTranscription(string trascription)
        {
            return trascription.RemoveMultispaces()
                .ToLowerInvariant()
                .WrapWithBracketsIfNeeded();
        }

        public List<string> PrepareExamples(string[] examples)
        {
            var result = new List<string>();

            foreach (var e in examples)
            {
                string item = e.RemoveMultispaces()
                    .ToLowerInvariant()
                    .AddPointIfNeeded();

                if (result.Contains(item))
                    continue;

                result.Add(item);
            }

            return result;
        }

        public List<string> PrepareTranslations(string[] translations)
        {
            var result = new List<string>();

            foreach (var e in translations)
            {
                string item = e.RemoveMultispaces()
                    .ToLowerInvariant();

                if (result.Contains(item))
                    continue;

                result.Add(item);
            }

            return result;
        }
    }
}
