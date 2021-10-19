using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BastardBot.Common
{
    public class BotConstants
    {

        public const string STEP_VALUES_NEW_INSULT_KEY = "NewInsult";
        public const string STEP_VALUES_MAIN_STATE_KEY = "STEP_VALUES_MAIN_STATE_KEY";

        public const string CMD_INSULTS_ONLY = "/listinsults";
        public const string CMD_INSULTS_AND_RESPONSES = "/listinsultsandresponses";
        public const string CMD_RETRAIN = "/retrain";

        public const string CHOICE_SAUL_GOODMAN = "Bring it On";


        public static string CHAT_CATEGORY_NEW_INSULT_REPLY { get { return "New insult reply"; } }
        public static string CHAT_CATEGORY_NEW_INSULT_START { get { return "New insult"; } }
        public static string CHAT_CATEGORY_ENDING_DISAPPOINT { get { return "Disappointed ending"; } }
        public static string CHAT_CATEGORY_MORE_INSULTS_QUESTION { get { return "More insults for response"; } }
        public static string CHAT_CATEGORY_INSULT_ADDED { get { return "Insult added"; } }
        public static string CHAT_CATEGORY_ALL_DONE { get { return "All done"; } }
        public static string CHAT_CATEGORY_NEXT_INSULT_PROMPT { get { return "Next insult"; } }

        public static string CHAT_CATEGORY_AS_I_SAID_BEFORE { get { return "As I said before"; } }
        public static string CHAT_CATEGORY_INSULT_LIKED { get { return "Liked insult"; } }
        public static string CHAT_CATEGORY_INSULT_DISLIKED { get { return "Disliked insult"; } }
    }
}
