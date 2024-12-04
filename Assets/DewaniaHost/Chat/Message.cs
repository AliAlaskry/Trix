using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DewaniaSession
{
    public partial class DewaniaGameData
    {
        public static class Chat
        {
            public static void UpdateChat(params Message[] messages)
            {
                Messages ??= new List<Message>();

                List<Message> newMessages = new List<Message>();
                foreach (Message message in messages)
                {
                    Message temp = Messages.Find(o => o.LocalMessage.Id == message.LocalMessage.Id);
                    if (temp == null)
                    {
                        newMessages.Add(message);
                        Messages.Add(message);
                    }
                    else
                        temp.LocalMessage.Sent = true;
                }

                newMessages = newMessages.OrderBy(o => o.SendTime).ToList();
                Messages = Messages.OrderBy(o => o.SendTime).ToList();

                foreach (Message message in newMessages)
                {
                    foreach (IGetNewMessage callback in Callbacks.OnGetNewMessage)
                        callback.OnGetMessage(message);
                }
            }

            public static void SendMessage(string content, bool wait, Action<Error> onFail)
            {
                Debugging.Print("Sending message = " + content);

                string id = Guid.NewGuid().ToString();
                LocalMessage localMessage = new LocalMessage(id, content);

                string localMessageJson = JsonConvert.SerializeObject(localMessage);

                JObject data = new JObject
                {
                    new JProperty("receiverId", GameId),
                    new JProperty("messageBody", localMessageJson),
                    new JProperty("type", "Game")
                };

                if (!wait)
                {
                    UpdateChat(new Message[] { new Message(id, LocalPlayer, localMessageJson) });
                }

                string url = NetworkInstance.Instance.Constants.BaseURL + @"/" + NetworkInstance.Instance.Constants.ChatsEndpoint;
                NetworkInstance.Instance.Http.SendRequset(url, data.ToString(Formatting.Indented), HttpStateEnum.SendMessage,
                    HttpMethod.POST, true, null, null, onFail);
            }

            public static List<Message> Messages { get; private set; }

            public static void Clear()
            {
                Messages = new List<Message>();
            }
        }

        public class Message
        {
            public Message(string id, DewaniaPlayer sender, string localMessageJson)
            {
                this.id = id;
                this.sender = new MessageSender(sender);
                this.localMessageJson = localMessageJson;
                this.localMessage = null;
            }

            [JsonProperty("id")]
            private string id;

            [JsonIgnore]
            public string Id
            {
                get { return id; }
            }

            [JsonProperty("sender")]
            private MessageSender sender;

            [JsonIgnore]
            public MessageSender Sender
            {
                get { return sender; }
            }

            // not set body dirctly to be able to display current message without waiting for host respose
            // make local message class contains local id for each message and body of message to be able to 
            // check if message display before or not by compare local id's (not host id's because i don't know 
            // it while sending)
            [JsonProperty("body")]
            private string localMessageJson;

            [JsonIgnore] private LocalMessage localMessage;
            [JsonIgnore]
            public LocalMessage LocalMessage
            {
                get
                {
                    if (localMessage != null) return localMessage;

                    try
                    {
                        localMessage = JsonConvert.DeserializeObject<LocalMessage>(localMessageJson);
                        return localMessage;
                    }
                    catch (Exception ex)
                    {
                        Debugging.Error("error while get local message = " + ex.Message);
                    }

                    return null;
                }
            }

            [JsonProperty("createdAt")]
            private DateTime sendTime;

            [JsonIgnore]
            public DateTime SendTime
            {
                get { return sendTime; }
            }
        }

        public class LocalMessage
        {
            public LocalMessage(string id, string content)
            {
                this.id = id;
                this.content = content;
                this.Sent = false;
            }

            [JsonProperty("id")]
            private string id;

            [JsonIgnore]
            public string Id
            {
                get { return id; }
            }

            [JsonProperty("content")]
            private string content;

            [JsonIgnore]
            public string Content
            {
                get { return content; }
            }

            [JsonIgnore]
            public bool Sent { get; set; }
        }

        public class MessageSender
        {
            public MessageSender(DewaniaPlayer player)
            {
                id = player.ID;
                name = player.Name;
                pic = player.Pic;
            }

            [JsonProperty("id")]
            private string id;

            [JsonIgnore]
            public string Id
            {
                get { return id; }
            }

            [JsonProperty("name")]
            private string name;

            [JsonIgnore]
            public string Name
            {
                get { return name; }
            }

            [JsonProperty("picture")]
            private string pic;

            [JsonIgnore]
            public string Pic
            {
                get { return pic; }
            }
        }
    }
}