using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Luis
{
    public partial class BastardTalk
    {
        public string Insult => Entities.insult?.FirstOrDefault();
    }
}
