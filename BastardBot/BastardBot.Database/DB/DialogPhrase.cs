using BastardBot.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace BastardBot.Common.DB
{
    public class DialogPhrase : BaseObjectWithText
    {
        public PhraseCategory Category { get; set; }
    }

    public class PhraseCategory : BaseObjectWithText
    { 
    }
}
