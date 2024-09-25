using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WhatsApp_Chatbot.Model;
using Action = WhatsApp_Chatbot.Model.Action;
using Section = WhatsApp_Chatbot.Model.Section;

namespace WhatsApp_Chatbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly string CRM_Document_Url = "https://crm-service-test.emata.io/api/documents";
        private readonly string WEBHOOK_VERIFY_TOKEN = "testing";
        private readonly string GRAPH_API_TOKEN = "EAACwi045M78BOyryTUXuKwtATxrP2GJA3z9IlQt9qz97vXZC2tZA1p10ZCoVa0DucUpQpa42vZCpTp6hrX62wP0EZBJOCZBcXwu25khE8AG4zO4ZBn77ZB0oxpKb2yw5iyEt56tWSBsyGJOutr5lfdZBZCHbM4a6Dbrm3UkZA1X0DnyjpBo7y37dj6NDwSARPE8VQrihcb61x82UbZAY18qGuv008hl1fGta7RByA3vGDyJZCq4qZB";
        private readonly string faceBookUrl = "https://graph.facebook.com/v19.0/";
        private readonly ILogger _logger;

        public ChatController(
          ILogger<ChatController> logger
            )
        {
            _logger = (ILogger)logger;
        }

        [HttpGet("/webhook")]
        public async Task<IActionResult> VerifyWebhook()
        {
            string mode = (string)this.HttpContext.Request.Query["hub.mode"];
            string token = (string)this.HttpContext.Request.Query["hub.verify_token"];
            string challenge = (string)this.HttpContext.Request.Query["hub.challenge"];
            this._logger.LogInformation("The request query is {token} ,{mode}, {challenge}", (object)token, (object)mode, (object)challenge);
            IActionResult actionResult = !(mode == "subscribe") || !(token == "testing") ? (IActionResult)this.StatusCode(403) : (IActionResult)this.Ok((object)challenge);
            mode = (string)null;
            token = (string)null;
            challenge = (string)null;
            return actionResult;
        }

        [HttpPost("/webhook")]
        public async Task<IActionResult> Webhook()
        {
            MetaWebhook request = new MetaWebhook();
            using (StreamReader reader = new StreamReader(this.HttpContext.Request.Body))
            {
                string bodyJson = await reader.ReadToEndAsync();
                this._logger.LogInformation("The request bodyJson is {bodyJson} ", (object)bodyJson);
                JsonDocument doc = JsonDocument.Parse(bodyJson);
                string obj = doc.RootElement.GetProperty("object").GetString();
                JsonElement entries = doc.RootElement.GetProperty("entry");
                this._logger.LogInformation("The deserializedJson for object and entries {object}, {entries} ", (object)obj, (object)entries);
                request = new MetaWebhook()
                {
                    Body = new Body()
                    {
                        Object = obj,
                        Entry = entries.Deserialize<List<Entry>>()
                    }
                };
                bodyJson = (string)null;
                doc = (JsonDocument)null;
                obj = (string)null;
                entries = new JsonElement();
            }
            MetaWebhook metaWebhook1 = request;
            Message message1;
            if (metaWebhook1 == null)
            {
                message1 = (Message)null;
            }
            else
            {
                Body body = metaWebhook1.Body;
                if (body == null)
                {
                    message1 = (Message)null;
                }
                else
                {
                    List<Entry> entry1 = body.Entry;
                    if (entry1 == null)
                    {
                        message1 = (Message)null;
                    }
                    else
                    {
                        Entry entry2 = entry1.First<Entry>();
                        if (entry2 == null)
                        {
                            message1 = (Message)null;
                        }
                        else
                        {
                            Change change = entry2.changes.First<Change>();
                            if (change == null)
                            {
                                message1 = (Message)null;
                            }
                            else
                            {
                                Value obj = change.value;
                                if (obj == null)
                                {
                                    message1 = (Message)null;
                                }
                                else
                                {
                                    List<Message> messages = obj.messages;
                                    message1 = messages != null ? messages.First<Message>() : (Message)null;
                                }
                            }
                        }
                    }
                }
            }
            Message message = message1;
            if (message?.type == "text" || message?.type == "button")
            {
                this._logger.LogInformation("The message type is {MessageType}", (object)message?.type);
                MetaWebhook metaWebhook2 = request;
                string str;
                if (metaWebhook2 == null)
                {
                    str = (string)null;
                }
                else
                {
                    Body body = metaWebhook2.Body;
                    if (body == null)
                    {
                        str = (string)null;
                    }
                    else
                    {
                        List<Entry> entry3 = body.Entry;
                        if (entry3 == null)
                        {
                            str = (string)null;
                        }
                        else
                        {
                            Entry entry4 = entry3.First<Entry>();
                            str = entry4 != null ? entry4.changes.First<Change>().value?.metadata?.phone_number_id : (string)null;
                        }
                    }
                }
                string businessPhoneNumberId = str;
                await this.SimulateConversation(message, businessPhoneNumberId, request);
                businessPhoneNumberId = (string)null;
            }
            if (message?.type == "image")
            {
                this._logger.LogInformation("The message type is {MessageType}", (object)message?.type);
                string imageId = message?.image?.id;
                this._logger.LogInformation("The ImageId is {Id}", (object)imageId);
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.GRAPH_API_TOKEN);
                    string url = this.faceBookUrl + "/" + imageId;
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            this._logger.LogInformation("The image response is {Image}", (object)response.Content);
                            byte[] ImageData = await response.Content.ReadAsByteArrayAsync();
                            this._logger.LogInformation("The bytes response is {Image}", (object)ImageData);
                            MultipartFormDataContent multiContent = new MultipartFormDataContent();
                            ByteArrayContent byteContent = new ByteArrayContent(ImageData, 0, ImageData.Length);
                            byteContent.Headers.Add("Content-Type", "image/jpeg");
                            multiContent.Add((HttpContent)byteContent, "UploadedFile", "FarmerPhoto");
                            multiContent.Add((HttpContent)new StringContent("Farmer Photo"), "documentType");
                            multiContent.Add((HttpContent)new StringContent(Guid.NewGuid().ToString()), "id");
                            multiContent.Add((HttpContent)new StringContent("e8d57839-cd20-48d4-b995-e4dc6ecdf660"), "contactId");
                            string accessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjlBREM1OEI2MzM3NDBDREM5RERBNTkzMEEyNzVERUE2IiwidHlwIjoiYXQrand0In0.eyJuYmYiOjE3MTM0NDMwMTQsImV4cCI6MTcxMzQ0NjYxNCwiaXNzIjoiaHR0cHM6Ly9hdXRoLWFwaS10ZXN0LmVtYXRhLmlvIiwiYXVkIjpbIkNoYXRib3QiLCJjb29wbWlzLXN5bmMiLCJDcmVkaXRTY29yZSIsIkNybSIsIkxvYW5Cb29rIiwibm90aWZpY2F0aW9uLXNlcnZpY2UiLCJQYXltZW50cyIsIlNtcy1EZWxpdmVyeSIsIlNtcy1Qcm9jZXNzb3IiXSwiY2xpZW50X2lkIjoiZW1hdGEtY2FzZWhhbmRsaW5nIiwiaWF0IjoxNzEzNDQzMDE0LCJzY29wZSI6WyJDaGF0Ym90IiwiY29vcG1pcy1zeW5jIiwiQ3JlZGl0U2NvcmUiLCJDcm0iLCJMb2FuQm9vayIsIm5vdGlmaWNhdGlvbi1zZXJ2aWNlIiwiUGF5bWVudHMiLCJTbXMtRGVsaXZlcnkiLCJTbXMtUHJvY2Vzc29yIl19.I7bWAbe_du081d3Fmc9UeWpikIMwnsM5R3kG4Z9E3HfvO2V37Dqtoq9o-Z6evX6T4twd8lUFZYrMoe2HyQ36sC9TgGp3CU-ubpUJjvu_uCU7aaMQ4GQq60LDzHtBwONFth4CgEZoc_f1ktfyHpye7WJuJeCWl9F8wK3okzoIVbI1OHn-NNCQQA-qWxkgQrrDr2op0yhgz7vTbjBaBqYSS-yKpBmTXM1TDwLIwGOWOG33ULILRjYwFIZq3CuyJycxBAt2ydMuslFeMFtwM1cnVZ4OKpIDsEzb9oSdpeOaIiafv7C9W3EJ0Xm4uVgu26CSul0PqDkRaY4ROM0BmWdj5w";
                            this._logger.LogInformation("Fetching access token  : {Token}", (object)accessToken);
                            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient()
                            {
                                Timeout = new TimeSpan(0, 0, 180)
                            })
                            {
                                this._logger.LogInformation("Sending to the Document Service");
                                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=l3iPy71otz");
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                                this._logger.LogInformation("Saving image to Documents with Url, {CRMURL}", (object)this.CRM_Document_Url);
                                return (IActionResult)this.Ok();
                            }
                        }
                        else
                            response = (HttpResponseMessage)null;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError("The network call has failed Exception : {ex}", (object)ex);
                        throw ex;
                    }
                    url = (string)null;
                }
                imageId = (string)null;
            }
            return (IActionResult)this.Ok();
        }

        private async Task SimulateConversation(
          Message? message,
          string? businessPhoneNumberId,
          MetaWebhook? metaWebhook)
        {
            this._logger.LogInformation("Starting to simylate the conversation for interactive Message");
            MetaWebhook metaWebhook1 = metaWebhook;
            string str;
            if (metaWebhook1 == null)
            {
                str = (string)null;
            }
            else
            {
                Body body = metaWebhook1.Body;
                if (body == null)
                {
                    str = (string)null;
                }
                else
                {
                    List<Entry> entry = body.Entry;
                    if (entry == null)
                    {
                        str = (string)null;
                    }
                    else
                    {
                        Value obj = entry.First<Entry>().changes.First<Change>().value;
                        str = obj != null ? obj.contacts.First<Contact>().profile?.name : (string)null;
                    }
                }
            }
            string name = str;
            string TextMessage = "*Hello " + name + ", welcome to Emata Uganda. What would you like to do?*";
            bool isGreeting = false;
            bool isPositive = false;
            bool isAgree = false;
            string[] greetingPatterns = new string[7]
            {
        "^hello\\b",
        "^hi\\b",
        "^hey\\b",
        "^howdy\\b",
        "^greetings\\b",
        "^what's up\\b",
        "^good (morning|afternoon|evening)\\b"
            };
            if (message?.type == "text" && message?.text != null)
                isGreeting = ((IEnumerable<string>)greetingPatterns).Any<string>((Func<string, bool>)(pattern => Regex.IsMatch(message?.text?.body?.ToLower(), pattern)));
            if (!isGreeting)
            {
                name = (string)null;
                TextMessage = (string)null;
                greetingPatterns = (string[])null;
            }
            else
            {
                Interactive interactive1 = new Interactive();
                interactive1.type = "list";
                interactive1.body = new InteractiveBody()
                {
                    text = TextMessage
                };
                interactive1.footer = new Footer()
                {
                    text = "To begin, tap Main Menu"
                };
                Action action = new Action();
                action.button = "Main Menu";
                List<Section> sectionList = new List<Section>();
                Section section = new Section();
                section.title = "Select from the options";
                List<Row> rowList = new List<Row>();
                rowList.Add(new Row()
                {
                    id = Guid.NewGuid().ToString(),
                    title = "Register farmer"
                });
                Row row1 = new Row();
                Guid guid = Guid.NewGuid();
                row1.id = guid.ToString();
                row1.title = "Aply for a loan";
                rowList.Add(row1);
                Row row2 = new Row();
                guid = Guid.NewGuid();
                row2.id = guid.ToString();
                row2.title = "Update documents";
                rowList.Add(row2);
                section.rows = rowList;
                sectionList.Add(section);
                action.sections = sectionList;
                interactive1.action = action;
                Interactive interactive = interactive1;
                await this.SendInteractiveMessage(interactive, message, businessPhoneNumberId, metaWebhook);
                interactive = (Interactive)null;
                name = (string)null;
                TextMessage = (string)null;
                greetingPatterns = (string[])null;
            }
        }

        private async Task SimulateConversations(
          Message message,
          string? businessPhoneNumberId,
          MetaWebhook? metaWebhook)
        {
            MetaWebhook metaWebhook1 = metaWebhook;
            string str;
            if (metaWebhook1 == null)
            {
                str = (string)null;
            }
            else
            {
                Body body = metaWebhook1.Body;
                if (body == null)
                {
                    str = (string)null;
                }
                else
                {
                    List<Entry> entry = body.Entry;
                    if (entry == null)
                    {
                        str = (string)null;
                    }
                    else
                    {
                        Value obj = entry.First<Entry>().changes.First<Change>().value;
                        str = obj != null ? obj.contacts.First<Contact>().profile?.name : (string)null;
                    }
                }
            }
            string name = str;
            string TextMessage = "Hello " + name + ", welcome to Emata Uganda";
            bool isGreeting = false;
            bool isPositive = false;
            bool isAgree = false;
            if (message?.type == "text" && message?.text != null)
            {
                string[] greetingPatterns = new string[7]
                {
          "^hello\\b",
          "^hi\\b",
          "^hey\\b",
          "^howdy\\b",
          "^greetings\\b",
          "^what's up\\b",
          "^good (morning|afternoon|evening)\\b"
                };
                isGreeting = ((IEnumerable<string>)greetingPatterns).Any<string>((Func<string, bool>)(pattern => Regex.IsMatch(message?.text?.body?.ToLower(), pattern)));
                string[] positivePatterns = new string[2]
                {
          "^(i\\s*am\\s*)?(fine|ok(?:ay)?|great|wonderful|good|better)\\b",
          "^(fine|ok(?:ay)?|great|wonderful|good|better)\\b"
                };
                isPositive = ((IEnumerable<string>)positivePatterns).Any<string>((Func<string, bool>)(pattern => Regex.IsMatch(message?.text?.body?.ToLower(), pattern)));
                string[] agreePatterns = new string[1]
                {
          "^(yes|yah|okay|sure|absolutely|indeed|definitely)\\b"
                };
                isAgree = ((IEnumerable<string>)agreePatterns).Any<string>((Func<string, bool>)(pattern => Regex.IsMatch(message?.text?.body?.ToLower(), pattern)));
                greetingPatterns = (string[])null;
                positivePatterns = (string[])null;
                agreePatterns = (string[])null;
            }
            if (message?.type == "button")
            {
                if (message?.button?.text == "Register farmer" || message?.button?.payload == "Register farmer")
                    TextMessage = "The farmer will receive an SMS from Emata with a code. Please enter it.";
                if (message?.button?.text == "Complete" || message?.button?.payload == "Complete")
                    TextMessage = "Successfully completed the registration for Walter Ruganzu.";
                if (message?.button?.text == "Cancel" || message?.button?.payload == "Cancel")
                    TextMessage = "Sorry Walter, but next time you can do better. I believe in you.";
                await this.SendMessage(TextMessage, message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else if (isGreeting)
            {
                TextMessage = "Hello *" + name + "*, How are you doing today?";
                isGreeting = false;
                await this.SendMessage(TextMessage, message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else if (isPositive)
            {
                isPositive = false;
                await this.SendMessage("*" + name + "*, Would you love to hear a joke ?", message, businessPhoneNumberId, metaWebhook);
                await this.MarkAsRead(message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else if (isAgree)
            {
                isAgree = false;
                await this.SendMessage("*Why did the scarecrow win an award?*\r\n\r\n*Because he was outstanding in his field!*", message, businessPhoneNumberId, metaWebhook);
                await this.MarkAsRead(message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else if (Regex.IsMatch(message?.text?.body?.ToLower(), "^(maybe|not sure|uncertain)\\b"))
            {
                await this.SendMessage("That's alright, *" + name + "*. Take your time to think it over. Let me know if you need any help.", message, businessPhoneNumberId, metaWebhook);
                await this.MarkAsRead(message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else if (Regex.IsMatch(message?.text?.body?.ToLower(), "^(joke|funny)\\b"))
            {
                await this.SendMessage("*Sure, here's one for you:*\r\n*Why don't scientists trust atoms? Because they make up everything!*", message, businessPhoneNumberId, metaWebhook);
                await this.MarkAsRead(message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else if (Regex.IsMatch(message?.text?.body?.ToLower(), "^(thank you|thanks)\\b"))
            {
                await this.SendMessage("You're welcome, *" + name + "*! If you need further assistance, feel free to ask.", message, businessPhoneNumberId, metaWebhook);
                await this.MarkAsRead(message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
            else
            {
                await this.SendMessage("Sorry, I didn't understand that.", message, businessPhoneNumberId, metaWebhook);
                await this.MarkAsRead(message, businessPhoneNumberId, metaWebhook);
                name = (string)null;
                TextMessage = (string)null;
            }
        }

        private async Task SendMessage(
          string text,
          Message? message,
          string? businessPhoneNumberId,
          MetaWebhook? metaWebhook)
        {
            this._logger.LogInformation("Sending Message through http call for {Text}", (object)text);
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.GRAPH_API_TOKEN);
                Data data1 = new Data();
                MetaWebhook metaWebhook1 = metaWebhook;
                string str1;
                if (metaWebhook1 == null)
                {
                    str1 = (string)null;
                }
                else
                {
                    Body body = metaWebhook1.Body;
                    if (body == null)
                    {
                        str1 = (string)null;
                    }
                    else
                    {
                        List<Entry> entry1 = body.Entry;
                        if (entry1 == null)
                        {
                            str1 = (string)null;
                        }
                        else
                        {
                            Entry entry2 = entry1.First<Entry>();
                            str1 = entry2 != null ? entry2.changes.First<Change>()?.value.messaging_product : (string)null;
                        }
                    }
                }
                data1.messaging_product = str1;
                MetaWebhook metaWebhook2 = metaWebhook;
                string str2;
                if (metaWebhook2 == null)
                {
                    str2 = (string)null;
                }
                else
                {
                    Body body = metaWebhook2.Body;
                    if (body == null)
                    {
                        str2 = (string)null;
                    }
                    else
                    {
                        List<Entry> entry3 = body.Entry;
                        if (entry3 == null)
                        {
                            str2 = (string)null;
                        }
                        else
                        {
                            Entry entry4 = entry3.First<Entry>();
                            if (entry4 == null)
                            {
                                str2 = (string)null;
                            }
                            else
                            {
                                Change change = entry4.changes.First<Change>();
                                if (change == null)
                                {
                                    str2 = (string)null;
                                }
                                else
                                {
                                    Value obj = change.value;
                                    if (obj == null)
                                    {
                                        str2 = (string)null;
                                    }
                                    else
                                    {
                                        List<Message> messages = obj.messages;
                                        str2 = messages != null ? messages.First<Message>().from : (string)null;
                                    }
                                }
                            }
                        }
                    }
                }
                data1.to = str2;
                data1.text = new Text() { body = text };
                data1.context = new Context()
                {
                    message_id = message?.id
                };
                Data data = data1;
                StringContent content = new StringContent(JsonSerializer.Serialize<Data>(data), Encoding.UTF8, "application/json");
                string url = this.faceBookUrl + businessPhoneNumberId + "/messages";
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(url, (HttpContent)content);
                    response.EnsureSuccessStatusCode();
                    response = (HttpResponseMessage)null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                data = (Data)null;
                content = (StringContent)null;
                url = (string)null;
            }
        }

        private async Task SendInteractiveMessage(
          Interactive interactiveMessage,
          Message? message,
          string? businessPhoneNumberId,
          MetaWebhook? metaWebhook)
        {
            this._logger.LogInformation("Sending Interactive Message through http call for {Text}", (object)interactiveMessage);
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.GRAPH_API_TOKEN);
                Data data1 = new Data();
                MetaWebhook metaWebhook1 = metaWebhook;
                string str1;
                if (metaWebhook1 == null)
                {
                    str1 = (string)null;
                }
                else
                {
                    Body body = metaWebhook1.Body;
                    if (body == null)
                    {
                        str1 = (string)null;
                    }
                    else
                    {
                        List<Entry> entry1 = body.Entry;
                        if (entry1 == null)
                        {
                            str1 = (string)null;
                        }
                        else
                        {
                            Entry entry2 = entry1.First<Entry>();
                            str1 = entry2 != null ? entry2.changes.First<Change>()?.value.messaging_product : (string)null;
                        }
                    }
                }
                data1.messaging_product = str1;
                MetaWebhook metaWebhook2 = metaWebhook;
                string str2;
                if (metaWebhook2 == null)
                {
                    str2 = (string)null;
                }
                else
                {
                    Body body = metaWebhook2.Body;
                    if (body == null)
                    {
                        str2 = (string)null;
                    }
                    else
                    {
                        List<Entry> entry3 = body.Entry;
                        if (entry3 == null)
                        {
                            str2 = (string)null;
                        }
                        else
                        {
                            Entry entry4 = entry3.First<Entry>();
                            if (entry4 == null)
                            {
                                str2 = (string)null;
                            }
                            else
                            {
                                Change change = entry4.changes.First<Change>();
                                if (change == null)
                                {
                                    str2 = (string)null;
                                }
                                else
                                {
                                    Value obj = change.value;
                                    if (obj == null)
                                    {
                                        str2 = (string)null;
                                    }
                                    else
                                    {
                                        List<Message> messages = obj.messages;
                                        str2 = messages != null ? messages.First<Message>().from : (string)null;
                                    }
                                }
                            }
                        }
                    }
                }
                data1.to = str2;
                data1.type = "interactive";
                data1.interactive = interactiveMessage;
                Data data = data1;
                string dataSerialized = JsonSerializer.Serialize<Data>(data);
                this._logger.LogInformation("The data json is : {Data}", (object)dataSerialized);
                StringContent content = new StringContent(dataSerialized, Encoding.UTF8, "application/json");
                string url = this.faceBookUrl + businessPhoneNumberId + "/messages";
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(url, (HttpContent)content);
                    response.EnsureSuccessStatusCode();
                    response = (HttpResponseMessage)null;
                }
                catch (Exception ex)
                {
                    this._logger.LogError("Failed to send message {Ex}", (object)ex.Message);
                    throw ex;
                }
                data = (Data)null;
                dataSerialized = (string)null;
                content = (StringContent)null;
                url = (string)null;
            }
        }

        private async Task MarkAsRead(
          Message message,
        string? businessPhoneNumberId,
          MetaWebhook? metaWebhook)
        {
            using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.GRAPH_API_TOKEN);
                Data data1 = new Data();
                MetaWebhook metaWebhook1 = metaWebhook;
                string str;
                if (metaWebhook1 == null)
                {
                    str = (string)null;
                }
                else
                {
                    Body body = metaWebhook1.Body;
                    if (body == null)
                    {
                        str = (string)null;
                    }
                    else
                    {
                        List<Entry> entry1 = body.Entry;
                        if (entry1 == null)
                        {
                            str = (string)null;
                        }
                        else
                        {
                            Entry entry2 = entry1.First<Entry>();
                            str = entry2 != null ? entry2.changes.First<Change>()?.value?.messaging_product : (string)null;
                        }
                    }
                }
                data1.messaging_product = str;
                data1.status = "read";
                data1.message_id = message?.id;
                Data data = data1;
                StringContent content = new StringContent(JsonSerializer.Serialize<Data>(data), Encoding.UTF8, "application/json");
                string url = "https://graph.facebook.com/v18.0/" + businessPhoneNumberId + "/messages";
                HttpResponseMessage response = await httpClient.PostAsync(url, (HttpContent)content);
                response.EnsureSuccessStatusCode();
                data = (Data)null;
                content = (StringContent)null;
                url = (string)null;
                response = (HttpResponseMessage)null;
            }
        }
    }
}
