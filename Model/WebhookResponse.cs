using Microsoft.AspNetCore;

namespace WhatsApp_Chatbot.Model
{
    public class WebhookResponse
    {
        public string? Body { get; set; }
    }

    public class WebhookRequest
    {
        public string? Body { get; set; }

        public string To { get; set; } = "256726092245";

        public string? From { get; set; }

        public string MessagingProduct { get; set; } = "whatsapp";
    }

    public class Value
    {
        public string? messaging_product { get; set; }

        public Metadata? metadata { get; set; }

        public List<Contact>? contacts { get; set; }

        public List<Message>? messages { get; set; }
    }
    public class User
    {
        public Guid Id { get; set; }

        public string Sender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }
    }

    public class Text
    {
        public string? body { get; set; }
    }

    public class Section
    {
        public string? title { get; set; }

        public List<Row>? rows { get; set; }
    }
    public class Row
    {
        public string? id { get; set; }

        public string? title { get; set; }

        public string? description { get; set; }
    }

    public class Profile
    {
        public string? name { get; set; }
    }

    public class MetaWebhook
    {
        public Body? Body { get; set; }
    }
    public class Metadata
    {
        public string? display_phone_number { get; set; }

        public string? phone_number_id { get; set; }
    }
    public class Message
    {
        public string? from { get; set; }

        public string? id { get; set; }

        public string? timestamp { get; set; }

        public Text? text { get; set; }

        public Image? image { get; set; }

        public Button? button { get; set; }

        public string? type { get; set; }
    }
    public class InteractiveBody
    {
        public string? text { get; set; }
    }
    public class Interactive
    {
        public string? type { get; set; }

        public Header? header { get; set; }

        public InteractiveBody? body { get; set; }

        public Footer? footer { get; set; }

        public Action? action { get; set; }
    }
    public class Image
    {
        public string? id { get; set; }

        public long? file_size { get; set; }

        public string? sha256 { get; set; }

        public string? messaging_product { get; set; }

        public string? url { get; set; }

        public string? mime_type { get; set; }
    }
    public class Header
    {
        public string? type { get; set; }

        public string? text { get; set; }
    }
    public class Footer
    {
        public string? text { get; set; }
    }
    public class Entry
    {
        public string? Id { get; set; }

        public List<Change> changes { get; set; }
    }
    public class Data
    {
        public string? messaging_product { get; set; }

        public string? message_id { get; set; }

        public string? to { get; set; }

        public string? type { get; set; }

        public Text? text { get; set; }

        public string? status { get; set; }

        public Context? context { get; set; }

        public Interactive? interactive { get; set; }
    }
    public class Context
    {
        public string? message_id { get; set; }
    }
    public class Contact
    {
        public Profile? profile { get; set; }

        public string? wa_id { get; set; }
    }
    public class Body
    {
        public string? Object { get; set; }

        public List<Entry>? Entry { get; set; }
    }

    public class Change
    {
        public Value? value { get; set; }

        public string? Field { get; set; } = "messages";
    }

    public class Button
    {
        public string? payload { get; set; }

        public string? text { get; set; }
    }

    public class Action
    {
        public string? button { get; set; }

        public List<Section>? sections { get; set; }
    }
}
