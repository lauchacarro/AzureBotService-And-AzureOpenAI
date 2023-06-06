using Azure.AI.OpenAI;

using System.Collections.Generic;

namespace EchoBot1
{
    public class CustomChatGptState
    {
        public ICollection<ChatMessage> Messages { get; set; }
    }
}
