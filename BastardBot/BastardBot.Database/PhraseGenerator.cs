using BastardBot.Common.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BastardBot.Common
{
    public class PhraseGenerator
    {
        private static Random rand = new Random();
        public static async Task<string> GetPhrase(string category, BastardDBContext context)
        {
            var cat = await context.ChatPhraseCategories.SingleOrDefaultAsync(p => p.Text == category);
            if (cat == null)
            {
                throw new ArgumentOutOfRangeException(nameof(category));
            }

            int phrasesWithCat = context.ChatPhrases.Where(p => p.Category == cat).Count();
            int toSkip = rand.Next(0, phrasesWithCat);
            DialogPhrase randomPhrase = null;
            if (phrasesWithCat == 0)
            {
                NoPhraseByCategory(category);
            }
            else if (phrasesWithCat == 1)
            {
                randomPhrase = await context.ChatPhrases.Where(p => p.Category == cat)
                    .SingleOrDefaultAsync();
            }
            else
            {
                randomPhrase = await context.ChatPhrases.Where(p => p.Category == cat)
                    .Skip(toSkip)
                    .Take(1)
                    .FirstAsync();
            }


            if (randomPhrase == null)
            {
                NoPhraseByCategory(category);
            }

            return randomPhrase.Text;
        }

        static void NoPhraseByCategory(string category)
        {
            throw new ArgumentOutOfRangeException(nameof(category), $"Couldn't find a phrase with category '{category}'");
        }
    }
}
