using BastardBot.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BastardBot.Common.DB
{
    public class Insult : BaseObjectWithText
    {
        public Insult(string insult) : base(insult) 
        {
        }

        public Insult() : base() { }

        [JsonIgnore]
        public InsultResponse ParentResponse { get; set; }

        //public static Insult GetRandom(BastardDBContext db)
        //{
        //    Random rand = new Random();
        //    int toSkip = rand.Next(0, db.NewInsults.Count());

        //    return db.NewInsults.Skip(toSkip).Take(1).First();
        //}
    }

    public class InsultResponse : BaseObjectWithText
    {
        public InsultResponse() { }
        public InsultResponse(string text) : base(text)
        {
            this.InsultTriggers = new List<Insult>();
        }

        public List<Insult> InsultTriggers { get; set; }
    }
}
