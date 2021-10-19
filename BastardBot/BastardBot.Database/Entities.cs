using System;
using System.Collections.Generic;
using System.Text;

namespace BastardBot.Common
{
    public class NewInsultQnA
    {
        public NewInsultQnA()
        {
            this.Insults = new List<string>();
        }

        public string InsultResponse { get; set; }
        public List<string> Insults { get; set; }

        public static NewInsultQnA FromNewInsult(string insult)
        {
            return new NewInsultQnA { Insults = new List<string> { insult } };
        }

        public bool IsReadyToSave
        {
            get 
            {
                return !string.IsNullOrEmpty(this.InsultResponse) && this.Insults.Count > 0;
            }
        }
    }

    public class MainDiagState
    {
        public bool AcceptedConditions { get; set; }
        public string LastInsult { get; set; }
    }
}
